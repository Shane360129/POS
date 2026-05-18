using System.ComponentModel.DataAnnotations;

namespace InvenFlow.Api.Models;

public enum DocStatus { Active = 0, Cancelled = 1 }

public class Purchase
{
    public int Id { get; set; }
    [Required, MaxLength(30)] public string Number { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCash { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Active;
    [MaxLength(500)] public string Note { get; set; } = "";
    public List<PurchaseItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PurchaseItem
{
    public int Id { get; set; }
    public int PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Qty { get; set; }
    public decimal UnitCost { get; set; }
}

public class Sale
{
    public int Id { get; set; }
    [Required, MaxLength(30)] public string Number { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalCost { get; set; }
    public bool IsCash { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Active;
    [MaxLength(500)] public string Note { get; set; } = "";
    public List<SaleItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public Sale? Sale { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
}

public class Payment
{
    public int Id { get; set; }
    [Required, MaxLength(30)] public string Number { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal Amount { get; set; }
    [MaxLength(50)] public string Method { get; set; } = "現金";
    public int? PurchaseId { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Active;
    [MaxLength(500)] public string Note { get; set; } = "";
}

public class Receipt
{
    public int Id { get; set; }
    [Required, MaxLength(30)] public string Number { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public decimal Amount { get; set; }
    [MaxLength(50)] public string Method { get; set; } = "現金";
    public int? SaleId { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Active;
    [MaxLength(500)] public string Note { get; set; } = "";
}

public class StockMovement
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    [Required, MaxLength(20)] public string Type { get; set; } = ""; // In / Out / AdjustIn / AdjustOut
    public int Qty { get; set; }
    public decimal UnitCost { get; set; }
    [MaxLength(30)] public string RefType { get; set; } = "";
    public int? RefId { get; set; }
    [MaxLength(500)] public string Note { get; set; } = "";
}
