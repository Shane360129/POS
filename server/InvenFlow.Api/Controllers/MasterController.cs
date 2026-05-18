using InvenFlow.Api.Data;
using InvenFlow.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvenFlow.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    readonly AppDbContext _db;
    public ProductsController(AppDbContext db) { _db = db; }

    [HttpGet] public async Task<IEnumerable<Product>> List() => await _db.Products.OrderBy(p => p.Code).ToListAsync();
    [HttpGet("{id}")] public async Task<ActionResult<Product>> Get(int id) => await _db.Products.FindAsync(id) is { } p ? p : NotFound();
    [HttpPost] public async Task<ActionResult<Product>> Create(Product p) { _db.Products.Add(p); await _db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = p.Id }, p); }
    [HttpPut("{id}")] public async Task<IActionResult> Update(int id, Product p)
    {
        if (id != p.Id) return BadRequest();
        var existing = await _db.Products.FindAsync(id); if (existing is null) return NotFound();
        existing.Code = p.Code; existing.Name = p.Name; existing.Category = p.Category;
        existing.Unit = p.Unit; existing.SafetyStock = p.SafetyStock; existing.Price = p.Price;
        await _db.SaveChangesAsync(); return NoContent();
    }
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Products.FindAsync(id); if (p is null) return NotFound();
        if (p.Stock != 0) return BadRequest(new { error = "庫存非零，無法刪除" });
        _db.Products.Remove(p); await _db.SaveChangesAsync(); return NoContent();
    }
}

[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    readonly AppDbContext _db;
    public SuppliersController(AppDbContext db) { _db = db; }
    [HttpGet] public async Task<IEnumerable<Supplier>> List() => await _db.Suppliers.OrderBy(s => s.Code).ToListAsync();
    [HttpGet("{id}")] public async Task<ActionResult<Supplier>> Get(int id) => await _db.Suppliers.FindAsync(id) is { } s ? s : NotFound();
    [HttpPost] public async Task<ActionResult<Supplier>> Create(Supplier s) { _db.Suppliers.Add(s); await _db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = s.Id }, s); }
    [HttpPut("{id}")] public async Task<IActionResult> Update(int id, Supplier s)
    {
        if (id != s.Id) return BadRequest();
        var existing = await _db.Suppliers.FindAsync(id); if (existing is null) return NotFound();
        existing.Code = s.Code; existing.Name = s.Name; existing.Contact = s.Contact;
        existing.Phone = s.Phone; existing.Email = s.Email;
        await _db.SaveChangesAsync(); return NoContent();
    }
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Suppliers.FindAsync(id); if (s is null) return NotFound();
        _db.Suppliers.Remove(s); await _db.SaveChangesAsync(); return NoContent();
    }
}

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    readonly AppDbContext _db;
    public CustomersController(AppDbContext db) { _db = db; }
    [HttpGet] public async Task<IEnumerable<Customer>> List() => await _db.Customers.OrderBy(c => c.Code).ToListAsync();
    [HttpGet("{id}")] public async Task<ActionResult<Customer>> Get(int id) => await _db.Customers.FindAsync(id) is { } c ? c : NotFound();
    [HttpPost] public async Task<ActionResult<Customer>> Create(Customer c) { _db.Customers.Add(c); await _db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = c.Id }, c); }
    [HttpPut("{id}")] public async Task<IActionResult> Update(int id, Customer c)
    {
        if (id != c.Id) return BadRequest();
        var existing = await _db.Customers.FindAsync(id); if (existing is null) return NotFound();
        existing.Code = c.Code; existing.Name = c.Name; existing.Contact = c.Contact;
        existing.Phone = c.Phone; existing.Email = c.Email; existing.CreditLimit = c.CreditLimit;
        await _db.SaveChangesAsync(); return NoContent();
    }
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Customers.FindAsync(id); if (c is null) return NotFound();
        _db.Customers.Remove(c); await _db.SaveChangesAsync(); return NoContent();
    }
}

[ApiController]
[Route("api/warehouses")]
public class WarehousesController : ControllerBase
{
    readonly AppDbContext _db;
    public WarehousesController(AppDbContext db) { _db = db; }
    [HttpGet] public async Task<IEnumerable<Warehouse>> List() => await _db.Warehouses.OrderBy(w => w.Code).ToListAsync();
    [HttpPost] public async Task<ActionResult<Warehouse>> Create(Warehouse w) { _db.Warehouses.Add(w); await _db.SaveChangesAsync(); return Ok(w); }
}

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    readonly AppDbContext _db;
    public AccountsController(AppDbContext db) { _db = db; }
    [HttpGet] public async Task<IEnumerable<Account>> List() => await _db.Accounts.OrderBy(a => a.Code).ToListAsync();
}
