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

        var allActivities = await _db.ActivityLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(100)
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
            AllActivities = allActivities,
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
        ViewBag.TotalCount = total;

        return View(logs);
    }

    [HttpGet]
    public async Task<IActionResult> GetActivityLogsJson(string? module, string? search, int limit = 50)
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
        var logs = await query.OrderByDescending(a => a.Timestamp).Take(limit).ToListAsync();
        return Json(logs);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Settings()
    {
        var settings = await _db.WebsiteSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new WebsiteSetting();
            _db.WebsiteSettings.Add(settings);
            await _db.SaveChangesAsync();
        }
        return View(settings);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(WebsiteSetting model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var settings = await _db.WebsiteSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            model.LastUpdatedAt = DateTime.UtcNow;
            _db.WebsiteSettings.Add(model);
        }
        else
        {
            settings.CompanyName = model.CompanyName;
            settings.Tagline = model.Tagline;
            settings.SupportEmail = model.SupportEmail;
            settings.SupportPhone = model.SupportPhone;
            settings.WhatsAppNumber = model.WhatsAppNumber;
            settings.HeadOfficeAddress = model.HeadOfficeAddress;
            settings.KarnalMillingAddress = model.KarnalMillingAddress;
            settings.MundraPortAddress = model.MundraPortAddress;
            settings.JnptPortAddress = model.JnptPortAddress;
            settings.AnnualMillingCapacityMT = model.AnnualMillingCapacityMT;
            settings.YearsOfHeritage = model.YearsOfHeritage;
            settings.CountriesExported = model.CountriesExported;
            settings.TopbarAnnouncement = model.TopbarAnnouncement;
            settings.LastUpdatedAt = DateTime.UtcNow;
        }

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "Admin",
            Action = "Website Settings Updated",
            Module = "Administration",
            Details = "Updated global company contact details, port logistics addresses, and public announcement.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Website CMS & company configuration saved successfully.";
        return RedirectToAction(nameof(Settings));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Users()
    {
        var users = await _db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        return View(users);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(string fullName, string username, string email, string password, UserRole role, string? department)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            TempData["ErrorMessage"] = "Username, email, and password are required.";
            return RedirectToAction(nameof(Users));
        }

        var normalizedEmail = email.Trim().ToLower();
        var normalizedUser = username.Trim().ToLower();

        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail || u.Username.ToLower() == normalizedUser))
        {
            TempData["ErrorMessage"] = "A user with this username or email already exists.";
            return RedirectToAction(nameof(Users));
        }

        var user = new User
        {
            FullName = fullName.Trim(),
            Username = normalizedUser,
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            Department = department,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "Admin",
            Action = "Staff User Created",
            Module = "Administration",
            Details = $"Created staff account for {user.FullName} ({user.Role}) in {user.Department ?? "Operations"}.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Staff account for '{user.FullName}' created successfully.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        if (User.Identity?.Name != null && user.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You cannot deactivate your own active session account.";
            return RedirectToAction(nameof(Users));
        }

        user.IsActive = !user.IsActive;

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "Admin",
            Action = user.IsActive ? "Staff Activated" : "Staff Deactivated",
            Module = "Administration",
            Details = $"{(user.IsActive ? "Activated" : "Deactivated")} staff login for {user.FullName} ({user.Username}).",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"User '{user.FullName}' status set to {(user.IsActive ? "Active" : "Inactive")}.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        if (User.Identity?.Name != null && user.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You cannot delete your own active administrator account.";
            return RedirectToAction(nameof(Users));
        }

        _db.Users.Remove(user);
        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "Admin",
            Action = "Staff User Deleted",
            Module = "Administration",
            Details = $"Deleted staff account: {user.FullName} ({user.Username}, {user.Role}).",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Staff user '{user.FullName}' deleted successfully.";
        return RedirectToAction(nameof(Users));
    }
}
