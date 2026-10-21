using ARROW_ARO_RICE.Models;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryStock> InventoryStocks => Set<InventoryStock>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();
    public DbSet<ExportShipment> ExportShipments => Set<ExportShipment>();
    public DbSet<ShipmentDocument> ShipmentDocuments => Set<ShipmentDocument>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<PaymentReceipt> PaymentReceipts => Set<PaymentReceipt>();
    public DbSet<OrderExpense> OrderExpenses => Set<OrderExpense>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<WebsiteSetting> WebsiteSettings => Set<WebsiteSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Quotation & SalesOrder 1-to-1 or 1-to-many relationship
        modelBuilder.Entity<Quotation>()
            .HasOne(q => q.ConvertedSalesOrder)
            .WithOne(so => so.Quotation)
            .HasForeignKey<SalesOrder>(so => so.QuotationId)
            .OnDelete(DeleteBehavior.SetNull);

        // SalesOrder relationships
        modelBuilder.Entity<SalesOrder>()
            .HasOne(so => so.Customer)
            .WithMany(c => c.SalesOrders)
            .HasForeignKey(so => so.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Invoices
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.SalesOrder)
            .WithMany(so => so.Invoices)
            .HasForeignKey(i => i.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Optional Product relationships with SetNull on delete
        modelBuilder.Entity<Inquiry>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<QuotationItem>()
            .HasOne(qi => qi.Product)
            .WithMany()
            .HasForeignKey(qi => qi.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<SalesOrderItem>()
            .HasOne(soi => soi.Product)
            .WithMany()
            .HasForeignKey(soi => soi.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        // InventoryStock composite key or unique index
        modelBuilder.Entity<InventoryStock>()
            .HasIndex(s => new { s.WarehouseId, s.ProductId, s.BatchNumber })
            .IsUnique();

        // Enum strings / conversions where useful
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Inquiry>()
            .Property(i => i.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Inquiry>()
            .Property(i => i.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<Quotation>()
            .Property(q => q.Status)
            .HasConversion<string>();

        modelBuilder.Entity<SalesOrder>()
            .Property(so => so.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ExportShipment>()
            .Property(s => s.CurrentMilestone)
            .HasConversion<string>();

        modelBuilder.Entity<ShipmentDocument>()
            .Property(d => d.DocType)
            .HasConversion<string>();

        modelBuilder.Entity<StockTransaction>()
            .Property(t => t.TransactionType)
            .HasConversion<string>();

        modelBuilder.Entity<PaymentReceipt>()
            .Property(p => p.PaymentMethod)
            .HasConversion<string>();

        modelBuilder.Entity<OrderExpense>()
            .Property(e => e.Category)
            .HasConversion<string>();
    }
}
