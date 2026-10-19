using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARROW_ARO_RICE.Models;

public class Quotation
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string QuotationNumber { get; set; } = string.Empty;

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? InquiryId { get; set; }
    public Inquiry? Inquiry { get; set; }

    [Required, MaxLength(150)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, MaxLength(150), EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CustomerCountry { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string PortOfLoading { get; set; } = "Mundra Port, Gujarat, India";

    [Required, MaxLength(100)]
    public string DestinationPort { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string IncoTerm { get; set; } = "CIF"; // FOB, CIF, CFR, EXW

    [Required, MaxLength(10)]
    public string Currency { get; set; } = "USD";

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal FreightCharges { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal InsuranceCharges { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherCharges { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Discount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GrandTotal { get; set; }

    public QuotationStatus Status { get; set; } = QuotationStatus.Draft;

    public DateTime QuotationDate { get; set; } = DateTime.UtcNow;

    public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddDays(15);

    [MaxLength(150)]
    public string PaymentTerms { get; set; } = "30% Advance T/T, 70% against BL Scan copy";

    [MaxLength(150)]
    public string InspectionAgency { get; set; } = "SGS / Bureau Veritas pre-shipment inspection at load port";

    public string? TermsAndConditions { get; set; }

    public string? BankDetails { get; set; }

    public string? Notes { get; set; }

    public int RevisionNumber { get; set; } = 1;

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigations
    public ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
    public SalesOrder? ConvertedSalesOrder { get; set; }
}

public class QuotationItem
{
    public int Id { get; set; }

    public int QuotationId { get; set; }
    public Quotation? Quotation { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    [Required, MaxLength(150)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(250)]
    public string SpecificationsSummary { get; set; } = "8.35mm AGL, 12% Moisture, 1% Broken max";

    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityMT { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPricePerMT { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(100)]
    public string Packaging { get; set; } = "50 Kg PP Bags";
}
