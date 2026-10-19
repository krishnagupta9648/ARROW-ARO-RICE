using System.ComponentModel.DataAnnotations;

namespace ARROW_ARO_RICE.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string ContactPerson { get; set; } = string.Empty;

    [Required, MaxLength(150), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? WhatsApp { get; set; }

    [Required, MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? TaxOrVatNumber { get; set; }

    [MaxLength(100)]
    public string? ImportLicenseNumber { get; set; }

    [MaxLength(100)]
    public string? PreferredDestinationPort { get; set; }

    public string Status { get; set; } = "Active Buyer"; // Prospect, Qualified, Active Buyer, Inactive

    public decimal CreditLimitUsd { get; set; } = 0;

    [MaxLength(100)]
    public string PaymentTerms { get; set; } = "30% Advance + 70% against BL Scan";

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigations
    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
