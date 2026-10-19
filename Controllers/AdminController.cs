using ARROW_ARO_RICE.Data;
using ARROW_ARO_RICE.Models;
using ARROW_ARO_RICE.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var totalInquiries = await _db.Inquiries.CountAsync();
        var activeLeads = await _db.Inquiries.CountAsync(i => i.Status != InquiryStatus.Lost && i.Status != InquiryStatus.Converted);
        var quotationsPending = await _db.Quotations.CountAsync(q => q.Status == QuotationStatus.PendingApproval || q.Status == QuotationStatus.SentToBuyer);
        var confirmedOrders = await _db.SalesOrders.CountAsync(so => so.Status == SalesOrderStatus.Confirmed || so.Status == SalesOrderStatus.Processing || so.Status == SalesOrderStatus.Packed);
        var shipmentsInTransit = await _db.ExportShipments.CountAsync(s => s.CurrentMilestone == ShipmentMilestone.InTransit || s.CurrentMilestone == ShipmentMilestone.VesselDeparted);

        var totalSalesVolumeMT = await _db.SalesOrders.Where(s => s.Status != SalesOrderStatus.Cancelled).SumAsync(s => s.TotalQuantityMT);
        var totalSalesRevenueUsd = await _db.SalesOrders.Where(s => s.Status != SalesOrderStatus.Cancelled).SumAsync(s => s.TotalAmount);
        
        var invoices = await _db.Invoices.ToListAsync();
        var totalReceivablesUsd = invoices.Sum(i => i.BalanceAmount);

        var totalStockAvailableMT = await _db.InventoryStocks.SumAsync(s => s.QuantityMT);

        var recentInquiries = await _db.Inquiries
            .OrderByDescending(i => i.CreatedAt)
            .Take(5)
            .ToListAsync();

        var activeShipments = await _db.ExportShipments
            .Include(s => s.SalesOrder)
            .ThenInclude(so => so!.Customer)
            .OrderByDescending(s => s.CreatedAt)
            .Take(4)
            .ToListAsync();

        var recentOrders = await _db.SalesOrders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .ToListAsync();

        var recentActivities = await _db.ActivityLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(8)
            .ToListAsync();

        var lowStockAlerts = await _db.InventoryStocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.QuantityMT <= s.ReorderLevelMT)
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            TotalInquiries = totalInquiries,
            ActiveLeads = activeLeads,
            QuotationsPending = quotationsPending,
            ConfirmedOrders = confirmedOrders,
            ShipmentsInTransit = shipmentsInTransit,
            TotalSalesVolumeMT = totalSalesVolumeMT,
            TotalSalesRevenueUsd = totalSalesRevenueUsd,
            TotalReceivablesUsd = totalReceivablesUsd,
            TotalStockAvailableMT = totalStockAvailableMT,
            RecentInquiries = recentInquiries,
            ActiveShipments = activeShipments,
            RecentOrders = recentOrders,
            RecentActivities = recentActivities,
            LowStockAlerts = lowStockAlerts
        };

        return View(vm);
    }

    public async Task<IActionResult> ActivityLogs(string? module, string? search, int page = 1)
    {
        var query = _db.ActivityLogs.AsQueryable();

        if (!string.IsNullOrEmpty(module) && module != "All")
        {
            query = query.Where(a => a.Module == module);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(a => a.Details.ToLower().Contains(s) || a.UserName.ToLower().Contains(s) || a.Action.ToLower().Contains(s));
        }

        const int pageSize = 20;
        var total = await query.CountAsync();
        var logs = await query.OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.SelectedModule = module ?? "All";
        ViewBag.SearchTerm = search ?? "";
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

        return View(logs);
    }
}
