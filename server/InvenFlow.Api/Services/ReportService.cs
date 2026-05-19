using InvenFlow.Api.Data;
using InvenFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InvenFlow.Api.Services;

public class ReportService
{
    readonly AppDbContext _db;
    public ReportService(AppDbContext db) { _db = db; }

    public async Task<object> DashboardAsync()
    {
        var sales = await _db.Sales.Where(s => s.Status == DocStatus.Active).ToListAsync();
        var purchases = await _db.Purchases.Where(p => p.Status == DocStatus.Active).ToListAsync();
        var receipts = (await _db.Receipts.Where(r => r.Status == DocStatus.Active).Select(r => r.Amount).ToListAsync()).Sum();
        var payments = (await _db.Payments.Where(p => p.Status == DocStatus.Active).Select(p => p.Amount).ToListAsync()).Sum();

        var products = await _db.Products.ToListAsync();
        var inventoryValue = products.Sum(p => p.Stock * p.AvgCost);
        var lowStock = products.Count(p => p.Stock < p.SafetyStock);

        var ar = sales.Where(s => !s.IsCash).Sum(s => s.TotalAmount) - receipts;
        var ap = purchases.Where(p => !p.IsCash).Sum(p => p.TotalAmount) - payments;

        var revenue = sales.Sum(s => s.TotalAmount);
        var cogs = sales.Sum(s => s.TotalCost);

        var today = DateTime.UtcNow.Date;
        var sales30 = sales.Where(s => s.Date >= today.AddDays(-30))
            .GroupBy(s => s.Date.Date)
            .Select(g => new { date = g.Key.ToString("MM-dd"), amount = g.Sum(x => x.TotalAmount) })
            .OrderBy(x => x.date).ToList();

        return new
        {
            revenue,
            cogs,
            grossProfit = revenue - cogs,
            grossMargin = revenue == 0 ? 0 : (revenue - cogs) / revenue,
            inventoryValue,
            lowStock,
            ar,
            ap,
            sales30,
            productCount = products.Count,
            customerCount = await _db.Customers.CountAsync(),
            supplierCount = await _db.Suppliers.CountAsync()
        };
    }

    public async Task<object> GrossProfitAsync(DateTime from, DateTime to)
    {
        var sales = await _db.Sales
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .Where(s => s.Status == DocStatus.Active && s.Date >= from && s.Date <= to)
            .ToListAsync();

        var rows = sales.SelectMany(s => s.Items)
            .GroupBy(i => i.Product!.Name)
            .Select(g => new
            {
                product = g.Key,
                qty = g.Sum(x => x.Qty),
                revenue = g.Sum(x => x.Qty * x.UnitPrice),
                cost = g.Sum(x => x.Qty * x.UnitCost),
                profit = g.Sum(x => x.Qty * (x.UnitPrice - x.UnitCost))
            })
            .OrderByDescending(x => x.profit)
            .ToList();

        return new
        {
            rows,
            totalRevenue = rows.Sum(r => r.revenue),
            totalCost = rows.Sum(r => r.cost),
            totalProfit = rows.Sum(r => r.profit)
        };
    }

    public async Task<object> ProductRankingAsync(int top = 10)
    {
        var items = await _db.SaleItems
            .Include(i => i.Product)
            .Include(i => i.Sale)
            .Where(i => i.Sale!.Status == DocStatus.Active)
            .ToListAsync();
        var ranking = items
            .GroupBy(i => i.Product!.Name)
            .Select(g => new
            {
                product = g.Key,
                qty = g.Sum(x => x.Qty),
                revenue = g.Sum(x => x.Qty * x.UnitPrice)
            })
            .OrderByDescending(x => x.revenue)
            .Take(top)
            .ToList();
        return new { ranking };
    }

    public async Task<object> InventoryTurnoverAsync()
    {
        var products = await _db.Products.ToListAsync();
        var saleItems = await _db.SaleItems
            .Include(i => i.Sale)
            .Where(i => i.Sale!.Status == DocStatus.Active)
            .ToListAsync();

        var soldByProduct = saleItems
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Qty));

        var rows = products.Select(p => new
        {
            code = p.Code,
            name = p.Name,
            stock = p.Stock,
            avgCost = p.AvgCost,
            value = p.Stock * p.AvgCost,
            sold = soldByProduct.GetValueOrDefault(p.Id, 0),
            turnover = p.Stock == 0 ? 0m : (decimal)soldByProduct.GetValueOrDefault(p.Id, 0) / p.Stock
        }).OrderByDescending(x => x.turnover).ToList();

        return new { rows };
    }

    public async Task<object> TrialBalanceAsync(DateTime from, DateTime to)
    {
        var lines = await _db.JournalLines
            .Include(l => l.JournalEntry)
            .Include(l => l.Account)
            .Where(l => l.JournalEntry!.Date >= from && l.JournalEntry.Date <= to)
            .ToListAsync();

        var rows = lines
            .GroupBy(l => new { l.AccountCode, l.Account!.Name, l.Account.Type })
            .Select(g => new
            {
                code = g.Key.AccountCode,
                name = g.Key.Name,
                type = g.Key.Type,
                debit = g.Sum(x => x.Debit),
                credit = g.Sum(x => x.Credit),
                balance = g.Sum(x => x.Debit) - g.Sum(x => x.Credit)
            })
            .OrderBy(r => r.code)
            .ToList();

        return new
        {
            rows,
            totalDebit = rows.Sum(r => r.debit),
            totalCredit = rows.Sum(r => r.credit),
            balanced = Math.Abs(rows.Sum(r => r.debit) - rows.Sum(r => r.credit)) < 0.01m
        };
    }
}
