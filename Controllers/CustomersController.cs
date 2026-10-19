using ARROW_ARO_RICE.Data;
using ARROW_ARO_RICE.Models;
using ARROW_ARO_RICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Controllers;

[Authorize(Roles = "Admin,Sales,Accounts")]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IExportService _exportService;

    public CustomersController(ApplicationDbContext db, IExportService exportService)
    {
        _db = db;
        _exportService = exportService;
    }

    public async Task<IActionResult> Index(string? search, string? country, int page = 1)
    {
        var query = _db.Customers.AsQueryable();

        if (!string.IsNullOrEmpty(country) && country != "All")
        {
            query = query.Where(c => c.Country == country);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c => c.CompanyName.ToLower().Contains(s) ||
                                     c.ContactPerson.ToLower().Contains(s) ||
                                     c.Email.ToLower().Contains(s) ||
                                     c.Country.ToLower().Contains(s) ||
                                     c.Phone.Contains(s));
        }

        const int pageSize = 15;
        var total = await query.CountAsync();
        var customers = await query
            .OrderBy(c => c.CompanyName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Countries = await _db.Customers.Select(c => c.Country).Distinct().OrderBy(c => c).ToListAsync();
        ViewBag.SelectedCountry = country ?? "All";
        ViewBag.SearchTerm = search ?? "";
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

        return View(customers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await _db.Customers
            .Include(c => c.Inquiries)
            .Include(c => c.Quotations)
            .Include(c => c.SalesOrders)
                .ThenInclude(so => so.Shipments)
            .Include(c => c.Invoices)
                .ThenInclude(inv => inv.Payments)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null) return NotFound();

        return View(customer);
    }

    public IActionResult Create()
    {
        return View(new Customer());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (!ModelState.IsValid)
        {
            return View(customer);
        }

        customer.CreatedAt = DateTime.UtcNow;
        _db.Customers.Add(customer);

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Customer Created",
            Module = "Customer",
            Details = $"New buyer {customer.CompanyName} ({customer.Country}) registered.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Buyer {customer.CompanyName} created successfully.";
        return RedirectToAction(nameof(Details), new { id = customer.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer model)
    {
        if (id != model.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        customer.CompanyName = model.CompanyName;
        customer.ContactPerson = model.ContactPerson;
        customer.Email = model.Email;
        customer.Phone = model.Phone;
        customer.WhatsApp = model.WhatsApp;
        customer.Country = model.Country;
        customer.City = model.City;
        customer.Address = model.Address;
        customer.TaxOrVatNumber = model.TaxOrVatNumber;
        customer.ImportLicenseNumber = model.ImportLicenseNumber;
        customer.PreferredDestinationPort = model.PreferredDestinationPort;
        customer.Status = model.Status;
        customer.CreditLimitUsd = model.CreditLimitUsd;
        customer.PaymentTerms = model.PaymentTerms;
        customer.Notes = model.Notes;

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Buyer profile updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Export()
    {
        var customers = await _db.Customers.OrderBy(c => c.CompanyName).ToListAsync();
        var headers = new[] { "Company Name", "Contact Person", "Email", "Phone", "WhatsApp", "Country", "City", "Tax/VAT", "Status", "Credit Limit (USD)", "Payment Terms" };
        var bytes = _exportService.ExportToCsv(customers, headers, c => new object?[]
        {
            c.CompanyName,
            c.ContactPerson,
            c.Email,
            c.Phone,
            c.WhatsApp,
            c.Country,
            c.City,
            c.TaxOrVatNumber,
            c.Status,
            c.CreditLimitUsd,
            c.PaymentTerms
        });

        return File(bytes, "text/csv", $"ArrowAroRice_Buyers_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _db.Customers
            .Include(c => c.Inquiries)
            .Include(c => c.Quotations)
            .Include(c => c.SalesOrders)
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null) return NotFound();

        // If customer has orders or quotes, archive rather than hard deleting to maintain financial ledger
        if (customer.SalesOrders.Any() || customer.Invoices.Any() || customer.Quotations.Any())
        {
            customer.Status = "Archived";
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Buyer '{customer.CompanyName}' has associated contracts/invoices and has been marked as Archived.";
        }
        else
        {
            if (customer.Inquiries.Any())
            {
                foreach (var inq in customer.Inquiries)
                {
                    inq.CustomerId = null;
                }
            }
            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Buyer '{customer.CompanyName}' was permanently deleted.";
        }

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Customer Deleted",
            Module = "Customer",
            Details = $"Customer {customer.CompanyName} ({customer.Country}) deleted/archived.",
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
