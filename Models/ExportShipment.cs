using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARROW_ARO_RICE.Models;

public class ExportShipment
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string ShipmentNumber { get; set; } = string.Empty;

    public int SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    [Required, MaxLength(100)]
    public string BookingNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ShippingLine { get; set; } = "Maersk Line"; // Maersk, MSC, CMA CGM, Hapag-Lloyd, HMM

    [MaxLength(120)]
    public string VesselName { get; set; } = "MSC MARINA";

    [MaxLength(50)]
    public string VoyageNumber { get; set; } = "V.2026E";

    [MaxLength(200)]
    public string ContainerNumbers { get; set; } = "MSCU7821941, MSCU7821942";

    [MaxLength(200)]
    public string SealNumbers { get; set; } = "ML-IN-98124, ML-IN-98125";

    [MaxLength(100)]
    public string BillOfLadingNumber { get; set; } = "BL-MND-DXB-98104";

    [Required, MaxLength(100)]
    public string PortOfLoading { get; set; } = "Mundra Port (INMUN1)";

    [Required, MaxLength(100)]
    public string PortOfDestination { get; set; } = "Jebel Ali Port, Dubai (AEJEA)";

    public DateTime? EstimatedDeparture { get; set; }

    public DateTime? ActualDeparture { get; set; }

    public DateTime? EstimatedArrival { get; set; }

    public DateTime? ActualArrival { get; set; }

    public ShipmentMilestone CurrentMilestone { get; set; } = ShipmentMilestone.OrderConfirmed;

    [MaxLength(100)]
    public string StatusDescription { get; set; } = "Vessel in Transit - High Seas";

    public string? TrackingNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigations
    public ICollection<ShipmentDocument> Documents { get; set; } = new List<ShipmentDocument>();
}

public class ShipmentDocument
{
    public int Id { get; set; }

    public int ExportShipmentId { get; set; }
    public ExportShipment? ExportShipment { get; set; }

    public DocumentType DocType { get; set; }

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string DocumentNumber { get; set; } = string.Empty;

    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string FilePath { get; set; } = string.Empty;

    public bool IsVerified { get; set; } = true;

    [MaxLength(150)]
    public string IssuingAuthority { get; set; } = "APEDA / Chamber of Commerce / Plant Quarantine Govt of India";

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
