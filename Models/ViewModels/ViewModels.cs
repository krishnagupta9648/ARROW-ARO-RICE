using System.ComponentModel.DataAnnotations;
using ARROW_ARO_RICE.Models;

namespace ARROW_ARO_RICE.Models.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class InquiryFormModel
{
    [Required(ErrorMessage = "Full Name is required"), MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? CompanyName { get; set; }

    [Required(ErrorMessage = "Valid business email is required"), MaxLength(150), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone / WhatsApp number is required"), MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Destination Country is required"), MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select Rice Variety")]
    public string RiceVariety { get; set; } = "1121 Steam Basmati Rice";

    public int? ProductId { get; set; }

    [Required(ErrorMessage = "Order Quantity in MT is required"), Range(1, 100000, ErrorMessage = "MOQ is 1 MT (Typical FCL is 25 MT)")]
    public decimal QuantityMT { get; set; } = 25.0m;

    [Required(ErrorMessage = "Destination Port is required"), MaxLength(100)]
    public string DestinationPort { get; set; } = string.Empty;

    public string IncoTerm { get; set; } = "CIF";

    public string PreferredPackaging { get; set; } = "50 Kg PP Bags";

    [Required(ErrorMessage = "Please enter inquiry specifications / requirements"), MinLength(10, ErrorMessage = "Please provide more details")]
    public string Message { get; set; } = string.Empty;
}

public class DashboardViewModel
{
    public int TotalInquiries { get; set; }
    public int ActiveLeads { get; set; }
    public int QuotationsPending { get; set; }
    public int ConfirmedOrders { get; set; }
    public int ShipmentsInTransit { get; set; }
    public decimal TotalSalesVolumeMT { get; set; }
    public decimal TotalSalesRevenueUsd { get; set; }
    public decimal TotalReceivablesUsd { get; set; }
    public decimal TotalStockAvailableMT { get; set; }

    public List<Inquiry> RecentInquiries { get; set; } = new();
    public List<ExportShipment> ActiveShipments { get; set; } = new();
    public List<SalesOrder> RecentOrders { get; set; } = new();
    public List<ActivityLog> RecentActivities { get; set; } = new();
    public List<ActivityLog> AllActivities { get; set; } = new();
    public List<InventoryStock> LowStockAlerts { get; set; } = new();
}

public class ProfitabilityItem
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal QuantityMT { get; set; }
    public decimal RevenueUsd { get; set; }
    public decimal ProcurementCostUsd { get; set; }
    public decimal LogisticsAndExpensesUsd { get; set; }
    public decimal NetProfitUsd => RevenueUsd - (ProcurementCostUsd + LogisticsAndExpensesUsd);
    public decimal ProfitMarginPercent => RevenueUsd > 0 ? (NetProfitUsd / RevenueUsd) * 100 : 0;
}

public class ProfitabilityReportViewModel
{
    public decimal TotalRevenueUsd { get; set; }
    public decimal TotalProcurementCostUsd { get; set; }
    public decimal TotalLogisticsExpensesUsd { get; set; }
    public decimal TotalNetProfitUsd { get; set; }
    public decimal OverallMarginPercent => TotalRevenueUsd > 0 ? (TotalNetProfitUsd / TotalRevenueUsd) * 100 : 0;
    public List<ProfitabilityItem> Orders { get; set; } = new();
}
