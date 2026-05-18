using InvenFlow.Api.Data;
using InvenFlow.Api.Dtos;
using InvenFlow.Api.Models;
using InvenFlow.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvenFlow.Api.Controllers;

[ApiController]
[Route("api/purchases")]
public class PurchasesController : ControllerBase
{
    readonly AppDbContext _db;
    readonly AccountingService _acct;
    public PurchasesController(AppDbContext db, AccountingService acct) { _db = db; _acct = acct; }

    [HttpGet]
    public async Task<object> List()
    {
        var rows = await _db.Purchases
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .Include(p => p.Items).ThenInclude(i => i.Product)
            .OrderByDescending(p => p.Date).ThenByDescending(p => p.Id)
            .ToListAsync();
        return rows.Select(p => new
        {
            p.Id, p.Number, p.Date, p.SupplierId, supplierName = p.Supplier?.Name,
            p.WarehouseId, warehouseName = p.Warehouse?.Name,
            p.TotalAmount, p.IsCash, status = p.Status.ToString(), p.Note,
            items = p.Items.Select(i => new { i.Id, i.ProductId, productName = i.Product?.Name, i.Qty, i.UnitCost })
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var p = await _db.Purchases
            .Include(x => x.Supplier).Include(x => x.Warehouse)
            .Include(x => x.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        return Ok(new
        {
            p.Id, p.Number, p.Date, p.SupplierId, supplierName = p.Supplier?.Name,
            p.WarehouseId, warehouseName = p.Warehouse?.Name,
            p.TotalAmount, p.IsCash, status = p.Status.ToString(), p.Note,
            items = p.Items.Select(i => new { i.Id, i.ProductId, productCode = i.Product?.Code, productName = i.Product?.Name, i.Qty, i.UnitCost })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseDto dto)
    {
        var purchase = new Purchase
        {
            Number = await _acct.NextNumberAsync("PO"),
            Date = dto.Date ?? DateTime.UtcNow,
            SupplierId = dto.SupplierId,
            WarehouseId = dto.WarehouseId,
            IsCash = dto.IsCash,
            Note = dto.Note ?? "",
            Items = dto.Items.Select(i => new PurchaseItem
            {
                ProductId = i.ProductId, Qty = i.Qty, UnitCost = i.UnitCost
            }).ToList()
        };
        await _acct.ApplyPurchaseAsync(purchase);
        return Ok(new { purchase.Id, purchase.Number });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        try { await _acct.CancelPurchaseAsync(id); return NoContent(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    readonly AppDbContext _db;
    readonly AccountingService _acct;
    public SalesController(AppDbContext db, AccountingService acct) { _db = db; _acct = acct; }

    [HttpGet]
    public async Task<object> List()
    {
        var rows = await _db.Sales
            .Include(s => s.Customer)
            .Include(s => s.Warehouse)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .OrderByDescending(s => s.Date).ThenByDescending(s => s.Id)
            .ToListAsync();
        return rows.Select(s => new
        {
            s.Id, s.Number, s.Date, s.CustomerId, customerName = s.Customer?.Name,
            s.WarehouseId, warehouseName = s.Warehouse?.Name,
            s.TotalAmount, s.TotalCost, s.IsCash, status = s.Status.ToString(), s.Note,
            items = s.Items.Select(i => new { i.Id, i.ProductId, productName = i.Product?.Name, i.Qty, i.UnitPrice, i.UnitCost })
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var s = await _db.Sales
            .Include(x => x.Customer).Include(x => x.Warehouse)
            .Include(x => x.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return NotFound();
        return Ok(new
        {
            s.Id, s.Number, s.Date, s.CustomerId, customerName = s.Customer?.Name,
            s.WarehouseId, warehouseName = s.Warehouse?.Name,
            s.TotalAmount, s.TotalCost, s.IsCash, status = s.Status.ToString(), s.Note,
            items = s.Items.Select(i => new { i.Id, i.ProductId, productCode = i.Product?.Code, productName = i.Product?.Name, i.Qty, i.UnitPrice, i.UnitCost })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleDto dto)
    {
        var sale = new Sale
        {
            Number = await _acct.NextNumberAsync("SO"),
            Date = dto.Date ?? DateTime.UtcNow,
            CustomerId = dto.CustomerId,
            WarehouseId = dto.WarehouseId,
            IsCash = dto.IsCash,
            Note = dto.Note ?? "",
            Items = dto.Items.Select(i => new SaleItem
            {
                ProductId = i.ProductId, Qty = i.Qty, UnitPrice = i.UnitPrice
            }).ToList()
        };
        try { await _acct.ApplySaleAsync(sale); return Ok(new { sale.Id, sale.Number }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        try { await _acct.CancelSaleAsync(id); return NoContent(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    readonly AppDbContext _db;
    readonly AccountingService _acct;
    public PaymentsController(AppDbContext db, AccountingService acct) { _db = db; _acct = acct; }

    [HttpGet]
    public async Task<object> List()
    {
        var rows = await _db.Payments.Include(p => p.Supplier)
            .OrderByDescending(p => p.Date).ToListAsync();
        return rows.Select(p => new
        {
            p.Id, p.Number, p.Date, p.SupplierId, supplierName = p.Supplier?.Name,
            p.Amount, p.Method, p.PurchaseId, status = p.Status.ToString(), p.Note
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        var p = new Payment
        {
            Number = await _acct.NextNumberAsync("PM"),
            Date = dto.Date ?? DateTime.UtcNow,
            SupplierId = dto.SupplierId,
            Amount = dto.Amount,
            Method = dto.Method,
            PurchaseId = dto.PurchaseId,
            Note = dto.Note ?? ""
        };
        await _acct.ApplyPaymentAsync(p);
        return Ok(new { p.Id, p.Number });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        try { await _acct.CancelPaymentAsync(id); return NoContent(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

[ApiController]
[Route("api/receipts")]
public class ReceiptsController : ControllerBase
{
    readonly AppDbContext _db;
    readonly AccountingService _acct;
    public ReceiptsController(AppDbContext db, AccountingService acct) { _db = db; _acct = acct; }

    [HttpGet]
    public async Task<object> List()
    {
        var rows = await _db.Receipts.Include(r => r.Customer)
            .OrderByDescending(r => r.Date).ToListAsync();
        return rows.Select(r => new
        {
            r.Id, r.Number, r.Date, r.CustomerId, customerName = r.Customer?.Name,
            r.Amount, r.Method, r.SaleId, status = r.Status.ToString(), r.Note
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReceiptDto dto)
    {
        var r = new Receipt
        {
            Number = await _acct.NextNumberAsync("RC"),
            Date = dto.Date ?? DateTime.UtcNow,
            CustomerId = dto.CustomerId,
            Amount = dto.Amount,
            Method = dto.Method,
            SaleId = dto.SaleId,
            Note = dto.Note ?? ""
        };
        await _acct.ApplyReceiptAsync(r);
        return Ok(new { r.Id, r.Number });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        try { await _acct.CancelReceiptAsync(id); return NoContent(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

[ApiController]
[Route("api/stock-movements")]
public class StockMovementsController : ControllerBase
{
    readonly AppDbContext _db;
    readonly AccountingService _acct;
    public StockMovementsController(AppDbContext db, AccountingService acct) { _db = db; _acct = acct; }

    [HttpGet]
    public async Task<object> List()
    {
        var rows = await _db.StockMovements
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .OrderByDescending(m => m.Date).Take(500)
            .ToListAsync();
        return rows.Select(m => new
        {
            m.Id, m.Date, productName = m.Product?.Name, warehouseName = m.Warehouse?.Name,
            m.Type, m.Qty, m.UnitCost, m.RefType, m.Note
        });
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] CreateAdjustmentDto dto)
    {
        if (dto.Delta == 0) return BadRequest(new { error = "調整數量不可為 0" });
        await _acct.ApplyAdjustmentAsync(dto.ProductId, dto.WarehouseId, dto.Delta, dto.Note);
        return NoContent();
    }
}

[ApiController]
[Route("api/journals")]
public class JournalsController : ControllerBase
{
    readonly AppDbContext _db;
    public JournalsController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<object> List()
    {
        var rows = await _db.JournalEntries
            .Include(j => j.Lines).ThenInclude(l => l.Account)
            .OrderByDescending(j => j.Date).ThenByDescending(j => j.Id)
            .Take(500)
            .ToListAsync();
        return rows.Select(j => new
        {
            j.Id, j.Number, j.Date, j.Description, j.RefType, j.RefId, j.IsReversal,
            lines = j.Lines.Select(l => new { l.AccountCode, accountName = l.Account?.Name, l.Debit, l.Credit, l.Description })
        });
    }
}
