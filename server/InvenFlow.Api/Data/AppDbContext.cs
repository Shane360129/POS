using InvenFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InvenFlow.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Product>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Supplier>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Customer>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Warehouse>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Purchase>().HasIndex(x => x.Number).IsUnique();
        b.Entity<Sale>().HasIndex(x => x.Number).IsUnique();
        b.Entity<Payment>().HasIndex(x => x.Number).IsUnique();
        b.Entity<Receipt>().HasIndex(x => x.Number).IsUnique();
        b.Entity<JournalEntry>().HasIndex(x => x.Number);

        foreach (var et in b.Model.GetEntityTypes())
        {
            foreach (var p in et.GetProperties())
            {
                if (p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?))
                    p.SetPrecision(18);
                if (p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?))
                    p.SetScale(4);
            }
        }

        b.Entity<Purchase>()
            .HasMany(x => x.Items).WithOne(x => x.Purchase!)
            .HasForeignKey(x => x.PurchaseId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Sale>()
            .HasMany(x => x.Items).WithOne(x => x.Sale!)
            .HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<JournalEntry>()
            .HasMany(x => x.Lines).WithOne(x => x.JournalEntry!)
            .HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
    }
}
