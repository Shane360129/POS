using InvenFlow.Api.Data;
using InvenFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InvenFlow.Api.Services;

/// <summary>
/// 進銷存核心：加權平均成本 (WAC) + 自動借貸分錄
/// </summary>
public class AccountingService
{
    readonly AppDbContext _db;
    public AccountingService(AppDbContext db) { _db = db; }

    // ===== 進貨：刷新 WAC、入庫、產生分錄 =====
    public async Task ApplyPurchaseAsync(Purchase purchase)
    {
        purchase.TotalAmount = 0;
        foreach (var item in purchase.Items)
        {
            var product = await _db.Products.FirstAsync(p => p.Id == item.ProductId);
            // WAC: new_avg = (old_qty*old_avg + qty*unit_cost) / (old_qty + qty)
            var oldQty = product.Stock;
            var oldAvg = product.AvgCost;
            var newQty = oldQty + item.Qty;
            product.AvgCost = newQty == 0 ? 0
                : Math.Round((oldQty * oldAvg + item.Qty * item.UnitCost) / newQty, 4);
            product.Stock = newQty;

            _db.StockMovements.Add(new StockMovement
            {
                Date = purchase.Date,
                ProductId = product.Id,
                WarehouseId = purchase.WarehouseId,
                Type = "In",
                Qty = item.Qty,
                UnitCost = item.UnitCost,
                RefType = "Purchase",
                Note = purchase.Number
            });

            purchase.TotalAmount += item.Qty * item.UnitCost;
        }

        if (purchase.Id == 0) _db.Purchases.Add(purchase);
        await _db.SaveChangesAsync();

        // 分錄
        var journal = new JournalEntry
        {
            Number = $"JE{purchase.Number}",
            Date = purchase.Date,
            Description = $"進貨 {purchase.Number}",
            RefType = "Purchase",
            RefId = purchase.Id,
            Lines =
            {
                new() { AccountCode = "1310", Debit = purchase.TotalAmount, Description = "存貨增加" },
                new() {
                    AccountCode = purchase.IsCash ? "1100" : "2100",
                    Credit = purchase.TotalAmount,
                    Description = purchase.IsCash ? "現金支付" : "應付帳款"
                }
            }
        };
        _db.JournalEntries.Add(journal);
        await _db.SaveChangesAsync();
    }

    // ===== 銷售：以當下 WAC 鎖定 COGS、產生分錄 =====
    public async Task ApplySaleAsync(Sale sale)
    {
        sale.TotalAmount = 0;
        sale.TotalCost = 0;
        foreach (var item in sale.Items)
        {
            var product = await _db.Products.FirstAsync(p => p.Id == item.ProductId);
            if (product.Stock < item.Qty)
                throw new InvalidOperationException($"{product.Name} 庫存不足（{product.Stock} < {item.Qty}）");

            item.UnitCost = product.AvgCost;
            product.Stock -= item.Qty;

            _db.StockMovements.Add(new StockMovement
            {
                Date = sale.Date,
                ProductId = product.Id,
                WarehouseId = sale.WarehouseId,
                Type = "Out",
                Qty = item.Qty,
                UnitCost = item.UnitCost,
                RefType = "Sale",
                Note = sale.Number
            });

            sale.TotalAmount += item.Qty * item.UnitPrice;
            sale.TotalCost += item.Qty * item.UnitCost;
        }

        if (sale.Id == 0) _db.Sales.Add(sale);
        await _db.SaveChangesAsync();

        var journal = new JournalEntry
        {
            Number = $"JE{sale.Number}",
            Date = sale.Date,
            Description = $"銷售 {sale.Number}",
            RefType = "Sale",
            RefId = sale.Id,
            Lines =
            {
                new() {
                    AccountCode = sale.IsCash ? "1100" : "1130",
                    Debit = sale.TotalAmount,
                    Description = sale.IsCash ? "現金收入" : "應收帳款"
                },
                new() { AccountCode = "4100", Credit = sale.TotalAmount, Description = "銷貨收入" },
                new() { AccountCode = "5100", Debit = sale.TotalCost, Description = "銷貨成本" },
                new() { AccountCode = "1310", Credit = sale.TotalCost, Description = "存貨減少" }
            }
        };
        _db.JournalEntries.Add(journal);
        await _db.SaveChangesAsync();
    }

    // ===== 付款 =====
    public async Task ApplyPaymentAsync(Payment p)
    {
        if (p.Id == 0) _db.Payments.Add(p);
        await _db.SaveChangesAsync();
        _db.JournalEntries.Add(new JournalEntry
        {
            Number = $"JE{p.Number}",
            Date = p.Date,
            Description = $"付款 {p.Number}",
            RefType = "Payment",
            RefId = p.Id,
            Lines =
            {
                new() { AccountCode = "2100", Debit = p.Amount, Description = "沖銷應付" },
                new() { AccountCode = "1100", Credit = p.Amount, Description = $"{p.Method}支付" }
            }
        });
        await _db.SaveChangesAsync();
    }

    // ===== 收款 =====
    public async Task ApplyReceiptAsync(Receipt r)
    {
        if (r.Id == 0) _db.Receipts.Add(r);
        await _db.SaveChangesAsync();
        _db.JournalEntries.Add(new JournalEntry
        {
            Number = $"JE{r.Number}",
            Date = r.Date,
            Description = $"收款 {r.Number}",
            RefType = "Receipt",
            RefId = r.Id,
            Lines =
            {
                new() { AccountCode = "1100", Debit = r.Amount, Description = $"{r.Method}收款" },
                new() { AccountCode = "1130", Credit = r.Amount, Description = "沖銷應收" }
            }
        });
        await _db.SaveChangesAsync();
    }

    // ===== 庫存調整：盤盈 / 盤虧 =====
    public async Task ApplyAdjustmentAsync(int productId, int warehouseId, int delta, string note)
    {
        var product = await _db.Products.FirstAsync(p => p.Id == productId);
        var cost = product.AvgCost;
        var amount = Math.Abs(delta) * cost;
        var isGain = delta > 0;

        product.Stock += delta;
        _db.StockMovements.Add(new StockMovement
        {
            Date = DateTime.UtcNow,
            ProductId = productId,
            WarehouseId = warehouseId,
            Type = isGain ? "AdjustIn" : "AdjustOut",
            Qty = Math.Abs(delta),
            UnitCost = cost,
            RefType = "Adjustment",
            Note = note
        });
        await _db.SaveChangesAsync();

        var number = $"ADJ{DateTime.UtcNow:yyyyMMddHHmmss}";
        _db.JournalEntries.Add(new JournalEntry
        {
            Number = $"JE{number}",
            Date = DateTime.UtcNow,
            Description = isGain ? $"盤盈 {note}" : $"盤虧 {note}",
            RefType = "Adjustment",
            Lines = isGain
                ? new()
                {
                    new() { AccountCode = "1310", Debit = amount, Description = "存貨盤盈" },
                    new() { AccountCode = "4900", Credit = amount, Description = "盤盈收入" }
                }
                : new()
                {
                    new() { AccountCode = "5900", Debit = amount, Description = "盤虧損失" },
                    new() { AccountCode = "1310", Credit = amount, Description = "存貨減少" }
                }
        });
        await _db.SaveChangesAsync();
    }

    // ===== 作廢：反向沖銷 =====
    public async Task CancelPurchaseAsync(int id)
    {
        var p = await _db.Purchases.Include(x => x.Items).FirstAsync(x => x.Id == id);
        if (p.Status == DocStatus.Cancelled) throw new InvalidOperationException("已作廢");

        foreach (var item in p.Items)
        {
            var product = await _db.Products.FirstAsync(x => x.Id == item.ProductId);
            // 簡化處理：作廢時不還原 WAC（避免後續銷售成本回推複雜化），只退庫存
            product.Stock -= item.Qty;
            _db.StockMovements.Add(new StockMovement
            {
                Date = DateTime.UtcNow, ProductId = product.Id, WarehouseId = p.WarehouseId,
                Type = "Out", Qty = item.Qty, UnitCost = item.UnitCost,
                RefType = "PurchaseCancel", Note = p.Number
            });
        }
        p.Status = DocStatus.Cancelled;
        await ReverseJournalAsync("Purchase", p.Id, $"作廢進貨 {p.Number}");
        await _db.SaveChangesAsync();
    }

    public async Task CancelSaleAsync(int id)
    {
        var s = await _db.Sales.Include(x => x.Items).FirstAsync(x => x.Id == id);
        if (s.Status == DocStatus.Cancelled) throw new InvalidOperationException("已作廢");

        foreach (var item in s.Items)
        {
            var product = await _db.Products.FirstAsync(x => x.Id == item.ProductId);
            product.Stock += item.Qty;
            _db.StockMovements.Add(new StockMovement
            {
                Date = DateTime.UtcNow, ProductId = product.Id, WarehouseId = s.WarehouseId,
                Type = "In", Qty = item.Qty, UnitCost = item.UnitCost,
                RefType = "SaleCancel", Note = s.Number
            });
        }
        s.Status = DocStatus.Cancelled;
        await ReverseJournalAsync("Sale", s.Id, $"作廢銷售 {s.Number}");
        await _db.SaveChangesAsync();
    }

    public async Task CancelPaymentAsync(int id)
    {
        var p = await _db.Payments.FirstAsync(x => x.Id == id);
        if (p.Status == DocStatus.Cancelled) throw new InvalidOperationException("已作廢");
        p.Status = DocStatus.Cancelled;
        await ReverseJournalAsync("Payment", p.Id, $"作廢付款 {p.Number}");
        await _db.SaveChangesAsync();
    }

    public async Task CancelReceiptAsync(int id)
    {
        var r = await _db.Receipts.FirstAsync(x => x.Id == id);
        if (r.Status == DocStatus.Cancelled) throw new InvalidOperationException("已作廢");
        r.Status = DocStatus.Cancelled;
        await ReverseJournalAsync("Receipt", r.Id, $"作廢收款 {r.Number}");
        await _db.SaveChangesAsync();
    }

    async Task ReverseJournalAsync(string refType, int refId, string desc)
    {
        var orig = await _db.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.RefType == refType && j.RefId == refId && !j.IsReversal);
        if (orig == null) return;

        _db.JournalEntries.Add(new JournalEntry
        {
            Number = $"REV{orig.Number}",
            Date = DateTime.UtcNow,
            Description = desc,
            RefType = refType,
            RefId = refId,
            IsReversal = true,
            Lines = orig.Lines.Select(l => new JournalLine
            {
                AccountCode = l.AccountCode,
                Debit = l.Credit,   // 反向
                Credit = l.Debit,
                Description = "反向沖銷：" + l.Description
            }).ToList()
        });
    }

    // ===== 編號產生 =====
    public async Task<string> NextNumberAsync(string prefix)
    {
        var today = DateTime.UtcNow.Date;
        var datePart = today.ToString("yyyyMMdd");
        var like = $"{prefix}{datePart}%";

        int seq = prefix switch
        {
            "PO" => await _db.Purchases.CountAsync(x => EF.Functions.Like(x.Number, like)) + 1,
            "SO" => await _db.Sales.CountAsync(x => EF.Functions.Like(x.Number, like)) + 1,
            "PM" => await _db.Payments.CountAsync(x => EF.Functions.Like(x.Number, like)) + 1,
            "RC" => await _db.Receipts.CountAsync(x => EF.Functions.Like(x.Number, like)) + 1,
            _ => 1
        };
        return $"{prefix}{datePart}{seq:D3}";
    }
}
