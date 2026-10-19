using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARROW_ARO_RICE.Models;

public class SalesOrder
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    public int? QuotationId { get; set; }
    public Quotation? Quotation { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime ExpectedShipmentDate { get; set; } = DateTime.UtcNow.AddDays(30);

    [Required, MaxLength(100)]
    public string PortOfLoading { get; set; } = "Mundra Port, Gujarat, India";

    [Required, MaxLength(100)]
    public string PortOfDestination { get; set; } = string.Empty;

    [MaxLength(20)]
    public string IncoTerm { get; set; } = "CIF";

    [MaxLength(150)]
    public string PaymentTerms { get; set; } = "30% Advance + 70% against BL";

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalQuantityMT { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Confirmed;

    [MaxLength(150)]
    public string? BuyerPoReference { get; set; }

    public string? SpecialInstructions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigations
    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
    public ICollection<ExportShipment> Shipments { get; set; } = new List<ExportShipment>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<OrderExpense> Expenses { get; set; } = new List<OrderExpense>();
}

public class SalesOrderItem
{
    public int Id { get; set; }

    public int SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    [Required, MaxLength(150)]
    public string ProductName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityMT { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPricePerMT { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(100)]
    public string Packaging { get; set; } = "50 Kg Non-Woven Bags";
}
