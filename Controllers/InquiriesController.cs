using ARROW_ARO_RICE.Data;
using ARROW_ARO_RICE.Models;
using ARROW_ARO_RICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Controllers;

[Authorize(Roles = "Admin,Sales")]
public class InquiriesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IExportService _exportService;

    public InquiriesController(ApplicationDbContext db, IExportService exportService)
    {
        _db = db;
        _exportService = exportService;
    }

    public async Task<IActionResult> Index(string? status, string? priority, string? search, int page = 1)
    {
        var query = _db.Inquiries
            .Include(i => i.AssignedToUser)
            .Include(i => i.Product)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<InquiryStatus>(status, out var sEnum))
        {
            query = query.Where(i => i.Status == sEnum);
        }

        if (!string.IsNullOrEmpty(priority) && Enum.TryParse<InquiryPriority>(priority, out var pEnum))
        {
            query = query.Where(i => i.Priority == pEnum);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(i => i.InquiryNumber.ToLower().Contains(s) ||
                                     i.FullName.ToLower().Contains(s) ||
                                     i.CompanyName.ToLower().Contains(s) ||
                                     i.Email.ToLower().Contains(s) ||
                                     i.Country.ToLower().Contains(s) ||
                                     i.RiceVariety.ToLower().Contains(s));
        }

        const int pageSize = 15;
        var total = await query.CountAsync();
        var inquiries = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.SelectedStatus = status ?? "";
        ViewBag.SelectedPriority = priority ?? "";
        ViewBag.SearchTerm = search ?? "";
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

        return View(inquiries);
    }

    public async Task<IActionResult> Details(int id)
    {
        var inquiry = await _db.Inquiries
            .Include(i => i.Customer)
            .Include(i => i.Product)
            .Include(i => i.AssignedToUser)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inquiry == null) return NotFound();

        ViewBag.SalesUsers = await _db.Users.Where(u => u.IsActive).ToListAsync();
        return View(inquiry);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, InquiryStatus status, string? internalNotes, DateTime? followUpDate)
    {
        var inquiry = await _db.Inquiries.FindAsync(id);
        if (inquiry == null) return NotFound();

        var oldStatus = inquiry.Status;
        inquiry.Status = status;
        if (!string.IsNullOrWhiteSpace(internalNotes))
        {
            inquiry.InternalNotes = (inquiry.InternalNotes + "\n[" + DateTime.UtcNow.ToString("dd MMM yyyy HH:mm") + "] " + internalNotes).Trim();
        }
        if (followUpDate.HasValue)
        {
            inquiry.FollowUpDate = followUpDate.Value;
        }
        inquiry.LastContactedAt = DateTime.UtcNow;

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Inquiry Status Updated",
            Module = "Inquiry",
            Details = $"Inquiry {inquiry.InquiryNumber} status changed from {oldStatus} to {status}.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Inquiry {inquiry.InquiryNumber} updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int id, int userId)
    {
        var inquiry = await _db.Inquiries.FindAsync(id);
        var user = await _db.Users.FindAsync(userId);
        if (inquiry == null || user == null) return NotFound();

        inquiry.AssignedToUserId = userId;

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Inquiry Assigned",
            Module = "Inquiry",
            Details = $"Inquiry {inquiry.InquiryNumber} assigned to {user.FullName}.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Inquiry assigned to {user.FullName}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Export(string? status)
    {
        var query = _db.Inquiries.Include(i => i.AssignedToUser).AsQueryable();
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<InquiryStatus>(status, out var sEnum))
        {
            query = query.Where(i => i.Status == sEnum);
        }

        var list = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();

        var headers = new[] { "Inquiry No", "Date", "Full Name", "Company", "Email", "Phone", "Country", "Rice Variety", "Quantity (MT)", "Destination Port", "IncoTerm", "Packaging", "Status", "Priority", "Assigned To" };
        var bytes = _exportService.ExportToCsv(list, headers, i => new object?[]
        {
            i.InquiryNumber,
            i.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
            i.FullName,
            i.CompanyName,
            i.Email,
            i.Phone,
            i.Country,
            i.RiceVariety,
            i.QuantityMT,
            i.DestinationPort,
            i.IncoTerm,
            i.PreferredPackaging,
            i.Status.ToString(),
            i.Priority.ToString(),
            i.AssignedToUser?.FullName ?? "Unassigned"
        });

        return File(bytes, "text/csv", $"ArrowAroRice_Inquiries_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
