using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARROW_ARO_RICE.Models;

public class InventoryStock
{
    public int Id { get; set; }

    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityMT { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ReservedMT { get; set; }

    [NotMapped]
    public decimal AvailableMT => QuantityMT - ReservedMT;

    [MaxLength(100)]
    public string BatchNumber { get; set; } = "BATCH-2026-001";

    [Column(TypeName = "decimal(18,2)")]
    public decimal MoistureTestedPercent { get; set; } = 12.0m;

    [MaxLength(100)]
    public string PackagingType { get; set; } = "50kg PP Bags";

    [Column(TypeName = "decimal(18,2)")]
    public decimal ReorderLevelMT { get; set; } = 50.0m;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class StockTransaction
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public StockTransactionType TransactionType { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityMT { get; set; }

    [MaxLength(100)]
    public string BatchNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ReferenceDocument { get; set; } // PO-..., SO-..., etc.

    public string? Notes { get; set; }

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
