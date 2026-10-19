using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARROW_ARO_RICE.Models;

public class Invoice
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public int SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxOrDuty { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GrandTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }

    [NotMapped]
    public decimal BalanceAmount => GrandTotal - PaidAmount;

    [MaxLength(50)]
    public string Status { get; set; } = "Unpaid"; // Unpaid, PartiallyPaid, Paid, Overdue

    [MaxLength(200)]
    public string? BankSwiftDetails { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PaymentReceipt> Payments { get; set; } = new List<PaymentReceipt>();
}

public class PaymentReceipt
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string ReceiptNumber { get; set; } = string.Empty;

    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.WireTransferTT;

    [MaxLength(100)]
    public string TransactionReference { get; set; } = string.Empty; // Swift UTR / LC Number

    [MaxLength(150)]
    public string? BankName { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class OrderExpense
{
    public int Id { get; set; }

    public int SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    public ExpenseCategory Category { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    [MaxLength(150)]
    public string VendorOrPayee { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? InvoiceOrReceiptNumber { get; set; }

    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ActivityLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(120)]
    public string UserName { get; set; } = "System";

    [Required, MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Module { get; set; } = string.Empty; // Inquiry, Quotation, SalesOrder, Shipment, Inventory, Finance

    [Required]
    public string Details { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
