using InvenFlow.Api.Models;
using InvenFlow.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace InvenFlow.Api.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(AppDbContext db, AccountingService acct)
    {
        if (await db.Products.AnyAsync()) return;

        await SeedAccountsAsync(db);
        await SeedMastersAsync(db);
        await db.SaveChangesAsync();

        await SeedPurchasesAsync(db, acct);
        await SeedSalesAsync(db, acct);
        await SeedPaymentsAsync(db, acct);
        await SeedReceiptsAsync(db, acct);
    }

    static Task SeedAccountsAsync(AppDbContext db)
    {
        db.Accounts.AddRange(
            new() { Code = "1100", Name = "現金", Type = "Asset" },
            new() { Code = "1130", Name = "應收帳款", Type = "Asset" },
            new() { Code = "1310", Name = "存貨", Type = "Asset" },
            new() { Code = "2100", Name = "應付帳款", Type = "Liability" },
            new() { Code = "4100", Name = "銷貨收入", Type = "Revenue" },
            new() { Code = "4900", Name = "存貨盤盈", Type = "Revenue" },
            new() { Code = "5100", Name = "銷貨成本", Type = "Expense" },
            new() { Code = "5900", Name = "存貨盤虧", Type = "Expense" }
        );
        return Task.CompletedTask;
    }

    static Task SeedMastersAsync(AppDbContext db)
    {
        db.Warehouses.AddRange(
            new() { Code = "WH-MAIN", Name = "主倉", Location = "台北市內湖區" },
            new() { Code = "WH-SOUTH", Name = "南部倉", Location = "高雄市岡山區" }
        );

        db.Suppliers.AddRange(
            new() { Code = "S001", Name = "聯欣國際", Contact = "陳經理", Phone = "02-2718-3000", Email = "[email protected]" },
            new() { Code = "S002", Name = "鴻達貿易", Contact = "林副理", Phone = "02-2511-8800", Email = "[email protected]" },
            new() { Code = "S003", Name = "智慧科技", Contact = "王協理", Phone = "03-578-9000", Email = "[email protected]" },
            new() { Code = "S004", Name = "東南物流", Contact = "黃主任", Phone = "07-731-2000", Email = "[email protected]" }
        );

        db.Customers.AddRange(
            new() { Code = "C001", Name = "誠品書店", Contact = "李採購", Phone = "02-8789-3388", Email = "[email protected]", CreditLimit = 500000 },
            new() { Code = "C002", Name = "全家便利商店", Contact = "張總監", Phone = "02-2725-2999", Email = "[email protected]", CreditLimit = 1000000 },
            new() { Code = "C003", Name = "小米科技", Contact = "周經理", Phone = "02-6602-9888", Email = "[email protected]", CreditLimit = 800000 },
            new() { Code = "C004", Name = "光南大批發", Contact = "吳店長", Phone = "07-201-6789", Email = "[email protected]", CreditLimit = 300000 },
            new() { Code = "C005", Name = "蝦皮購物", Contact = "蔡採購", Phone = "02-7740-7777", Email = "[email protected]", CreditLimit = 1500000 }
        );

        db.Products.AddRange(
            new() { Code = "P001", Name = "藍牙耳機 Pro", Category = "電子產品", Unit = "副", SafetyStock = 20, Price = 2800 },
            new() { Code = "P002", Name = "USB-C 快充線", Category = "配件", Unit = "條", SafetyStock = 50, Price = 380 },
            new() { Code = "P003", Name = "行動電源 10000mAh", Category = "電子產品", Unit = "顆", SafetyStock = 30, Price = 890 },
            new() { Code = "P004", Name = "機械式鍵盤", Category = "電子產品", Unit = "把", SafetyStock = 15, Price = 3200 },
            new() { Code = "P005", Name = "無線滑鼠", Category = "電子產品", Unit = "個", SafetyStock = 25, Price = 680 },
            new() { Code = "P006", Name = "筆電支架", Category = "配件", Unit = "個", SafetyStock = 30, Price = 590 },
            new() { Code = "P007", Name = "螢幕保護貼", Category = "配件", Unit = "片", SafetyStock = 100, Price = 250 },
            new() { Code = "P008", Name = "手機殼 透明", Category = "配件", Unit = "個", SafetyStock = 80, Price = 180 },
            new() { Code = "P009", Name = "智慧手錶", Category = "電子產品", Unit = "支", SafetyStock = 10, Price = 5800 },
            new() { Code = "P010", Name = "藍牙喇叭", Category = "電子產品", Unit = "台", SafetyStock = 15, Price = 1980 },
            new() { Code = "P011", Name = "記憶卡 128G", Category = "儲存裝置", Unit = "張", SafetyStock = 40, Price = 450 },
            new() { Code = "P012", Name = "外接硬碟 1TB", Category = "儲存裝置", Unit = "顆", SafetyStock = 20, Price = 2200 }
        );
        return Task.CompletedTask;
    }

    static async Task SeedPurchasesAsync(AppDbContext db, AccountingService acct)
    {
        var products = await db.Products.ToDictionaryAsync(p => p.Code);
        var sups = await db.Suppliers.ToDictionaryAsync(s => s.Code);
        var wh = (await db.Warehouses.FirstAsync(w => w.Code == "WH-MAIN")).Id;
        var today = DateTime.UtcNow.Date;

        var po = new (string sup, int daysAgo, bool cash, (string code, int qty, decimal cost)[] items)[]
        {
            ("S001", 45, false, new[] { ("P001", 30, 1800m), ("P002", 100, 200m) }),
            ("S002", 38, false, new[] { ("P003", 40, 550m), ("P005", 50, 400m) }),
            ("S003", 30, true,  new[] { ("P004", 20, 2000m) }),
            ("S001", 25, false, new[] { ("P006", 40, 350m), ("P007", 200, 120m), ("P008", 150, 80m) }),
            ("S004", 20, false, new[] { ("P009", 15, 3800m), ("P010", 25, 1200m) }),
            ("S002", 15, false, new[] { ("P011", 60, 280m), ("P012", 25, 1500m) }),
            ("S001", 10, true,  new[] { ("P001", 20, 1850m) }),
            ("S003", 5,  false, new[] { ("P004", 10, 2050m), ("P005", 30, 410m) })
        };

        int seq = 1;
        foreach (var p in po)
        {
            var purchase = new Purchase
            {
                Number = $"PO{today.AddDays(-p.daysAgo):yyyyMMdd}{seq++:D3}",
                Date = today.AddDays(-p.daysAgo),
                SupplierId = sups[p.sup].Id,
                WarehouseId = wh,
                IsCash = p.cash,
                Items = p.items.Select(i => new PurchaseItem
                {
                    ProductId = products[i.code].Id,
                    Qty = i.qty,
                    UnitCost = i.cost
                }).ToList()
            };
            await acct.ApplyPurchaseAsync(purchase);
        }
    }

    static async Task SeedSalesAsync(AppDbContext db, AccountingService acct)
    {
        var products = await db.Products.ToDictionaryAsync(p => p.Code);
        var custs = await db.Customers.ToDictionaryAsync(c => c.Code);
        var wh = (await db.Warehouses.FirstAsync(w => w.Code == "WH-MAIN")).Id;
        var today = DateTime.UtcNow.Date;

        var so = new (string cust, int daysAgo, bool cash, (string code, int qty)[] items)[]
        {
            ("C001", 35, false, new[] { ("P001", 5), ("P002", 20) }),
            ("C002", 32, false, new[] { ("P003", 10), ("P005", 8) }),
            ("C001", 28, true,  new[] { ("P006", 6), ("P007", 30) }),
            ("C003", 24, false, new[] { ("P004", 4) }),
            ("C002", 20, false, new[] { ("P008", 50), ("P011", 20) }),
            ("C004", 18, true,  new[] { ("P009", 3), ("P010", 5) }),
            ("C005", 15, false, new[] { ("P012", 8), ("P004", 3) }),
            ("C001", 12, false, new[] { ("P001", 3), ("P003", 5) }),
            ("C003", 9,  true,  new[] { ("P005", 6) }),
            ("C002", 7,  false, new[] { ("P002", 30), ("P007", 50), ("P008", 40) }),
            ("C004", 5,  false, new[] { ("P010", 4) }),
            ("C005", 3,  true,  new[] { ("P011", 15), ("P012", 5) }),
            ("C001", 1,  false, new[] { ("P006", 5), ("P009", 2) })
        };

        int seq = 1;
        foreach (var s in so)
        {
            var sale = new Sale
            {
                Number = $"SO{today.AddDays(-s.daysAgo):yyyyMMdd}{seq++:D3}",
                Date = today.AddDays(-s.daysAgo),
                CustomerId = custs[s.cust].Id,
                WarehouseId = wh,
                IsCash = s.cash,
                Items = s.items.Select(i =>
                {
                    var prod = products[i.code];
                    return new SaleItem
                    {
                        ProductId = prod.Id,
                        Qty = i.qty,
                        UnitPrice = prod.Price
                    };
                }).ToList()
            };
            await acct.ApplySaleAsync(sale);
        }
    }

    static async Task SeedPaymentsAsync(AppDbContext db, AccountingService acct)
    {
        var sups = await db.Suppliers.ToDictionaryAsync(s => s.Code);
        var today = DateTime.UtcNow.Date;
        var p1 = new Payment { Number = $"PM{today.AddDays(-30):yyyyMMdd}001", Date = today.AddDays(-30), SupplierId = sups["S001"].Id, Amount = 60000, Method = "電匯" };
        var p2 = new Payment { Number = $"PM{today.AddDays(-22):yyyyMMdd}001", Date = today.AddDays(-22), SupplierId = sups["S002"].Id, Amount = 40000, Method = "支票" };
        var p3 = new Payment { Number = $"PM{today.AddDays(-8):yyyyMMdd}001", Date = today.AddDays(-8), SupplierId = sups["S004"].Id, Amount = 50000, Method = "電匯" };
        await acct.ApplyPaymentAsync(p1);
        await acct.ApplyPaymentAsync(p2);
        await acct.ApplyPaymentAsync(p3);
    }

    static async Task SeedReceiptsAsync(AppDbContext db, AccountingService acct)
    {
        var custs = await db.Customers.ToDictionaryAsync(c => c.Code);
        var today = DateTime.UtcNow.Date;
        var r1 = new Receipt { Number = $"RC{today.AddDays(-26):yyyyMMdd}001", Date = today.AddDays(-26), CustomerId = custs["C001"].Id, Amount = 30000, Method = "電匯" };
        var r2 = new Receipt { Number = $"RC{today.AddDays(-18):yyyyMMdd}001", Date = today.AddDays(-18), CustomerId = custs["C002"].Id, Amount = 25000, Method = "ATM" };
        var r3 = new Receipt { Number = $"RC{today.AddDays(-10):yyyyMMdd}001", Date = today.AddDays(-10), CustomerId = custs["C003"].Id, Amount = 18000, Method = "電匯" };
        var r4 = new Receipt { Number = $"RC{today.AddDays(-4):yyyyMMdd}001", Date = today.AddDays(-4), CustomerId = custs["C005"].Id, Amount = 22000, Method = "支票" };
        await acct.ApplyReceiptAsync(r1);
        await acct.ApplyReceiptAsync(r2);
        await acct.ApplyReceiptAsync(r3);
        await acct.ApplyReceiptAsync(r4);
    }
}
