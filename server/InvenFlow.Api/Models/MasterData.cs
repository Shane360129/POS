using System.ComponentModel.DataAnnotations;

namespace InvenFlow.Api.Models;

public class Product
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string Code { get; set; } = "";
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(50)] public string Category { get; set; } = "";
    [MaxLength(20)] public string Unit { get; set; } = "個";
    public int SafetyStock { get; set; }
    public decimal AvgCost { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Supplier
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string Code { get; set; } = "";
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(100)] public string Contact { get; set; } = "";
    [MaxLength(50)] public string Phone { get; set; } = "";
    [MaxLength(200)] public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Customer
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string Code { get; set; } = "";
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(100)] public string Contact { get; set; } = "";
    [MaxLength(50)] public string Phone { get; set; } = "";
    [MaxLength(200)] public string Email { get; set; } = "";
    public decimal CreditLimit { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Warehouse
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string Code { get; set; } = "";
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(200)] public string Location { get; set; } = "";
}

public class Account
{
    [Key, MaxLength(10)] public string Code { get; set; } = "";
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    [Required, MaxLength(20)] public string Type { get; set; } = "";
}
