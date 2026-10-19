using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARROW_ARO_RICE.Models;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Category { get; set; } = "Basmati"; // Basmati, Non-Basmati, Organic, 100% Broken

    [MaxLength(100)]
    public string Variety { get; set; } = "1121"; // 1121, 1509, Traditional, Sugandha, Sharbati, PR11, Sona Masoori, IR64

    [MaxLength(100)]
    public string ProcessingType { get; set; } = "Steam"; // Steam, Creamy Sella, Golden Sella, Raw/Milled, Parboiled

    [Column(TypeName = "decimal(18,2)")]
    public decimal AverageLengthMm { get; set; } = 8.35m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal MoistureMaxPercent { get; set; } = 12.5m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal BrokenMaxPercent { get; set; } = 1.0m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ChalkyMaxPercent { get; set; } = 2.0m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ForeignMatterMaxPercent { get; set; } = 0.05m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal AdmixtureMaxPercent { get; set; } = 5.0m;

    [MaxLength(100)]
    public string ElongationRatio { get; set; } = "2.0x - 2.5x";

    [MaxLength(100)]
    public string AromaGrade { get; set; } = "Exquisite Natural Aroma";

    [MaxLength(100)]
    public string KettWhiteness { get; set; } = "32 - 34 Kett";

    [Required]
    public string PackagingOptions { get; set; } = "1kg, 5kg, 10kg, 20kg, 25kg, 50kg in BOPP, Non-Woven, Jute & PP Bags";

    [Column(TypeName = "decimal(18,2)")]
    public decimal MinOrderQuantityMT { get; set; } = 25.0m; // 1 FCL 20ft container approx 25 MT

    [Column(TypeName = "decimal(18,2)")]
    public decimal DefaultFobPricePerMT { get; set; } = 1050.0m;

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public string Description { get; set; } = string.Empty;

    public string? Highlights { get; set; }

    [MaxLength(255)]
    public string ImageUrl { get; set; } = "/images/rice-placeholder.jpg";

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigations
    public ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();
}
