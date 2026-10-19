using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARROW_ARO_RICE.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContactPerson { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(150), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(100)]
    public string SupplierType { get; set; } = "Mandi Agent"; // Paddy Farmer, Mandi Agent, Processing Mill, Packaging Manufacturer

    [MaxLength(100)]
    public string? GstOrTaxNumber { get; set; }

    public int RatingStars { get; set; } = 5;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}

public class PurchaseOrder
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string PoNumber { get; set; } = string.Empty;

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityMT { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitRatePerMT { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "INR";

    [MaxLength(50)]
    public string Status { get; set; } = "Ordered"; // Ordered, InTransit, Received, QualityInspected, Closed

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? ActualReceivedDate { get; set; }

    [MaxLength(100)]
    public string? BatchNumberGenerated { get; set; }

    public string? QualityCheckRemarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
