using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARROW_ARO_RICE.Models;

public class Inquiry
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string InquiryNumber { get; set; } = string.Empty;

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, MaxLength(150), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string RiceVariety { get; set; } = string.Empty;

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityMT { get; set; }

    [MaxLength(100)]
    public string DestinationPort { get; set; } = string.Empty;

    [MaxLength(50)]
    public string IncoTerm { get; set; } = "CIF"; // FOB, CIF, CFR, EXW

    [MaxLength(150)]
    public string PreferredPackaging { get; set; } = "50 Kg PP Bags";

    [Required]
    public string Message { get; set; } = string.Empty;

    public InquiryStatus Status { get; set; } = InquiryStatus.New;

    public InquiryPriority Priority { get; set; } = InquiryPriority.Normal;

    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public DateTime? FollowUpDate { get; set; }

    public string? InternalNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastContactedAt { get; set; }
}
