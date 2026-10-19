using System.Diagnostics;
using ARROW_ARO_RICE.Data;
using ARROW_ARO_RICE.Models;
using ARROW_ARO_RICE.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext db, ILogger<HomeController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var featuredProducts = await _db.Products
            .Where(p => p.IsActive && p.IsFeatured)
            .Take(6)
            .ToListAsync();

        ViewBag.FeaturedProducts = featuredProducts;
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public async Task<IActionResult> Products(string? category, string? search)
    {
        var query = _db.Products.Where(p => p.IsActive).AsQueryable();

        if (!string.IsNullOrEmpty(category) && category != "All")
        {
            query = query.Where(p => p.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) || 
                                     p.Variety.ToLower().Contains(s) || 
                                     p.Code.ToLower().Contains(s));
        }

        var products = await query.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
        ViewBag.SelectedCategory = category ?? "All";
        ViewBag.SearchTerm = search ?? "";
        return View(products);
    }

    public async Task<IActionResult> ProductDetail(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null)
        {
            return NotFound();
        }

        var relatedProducts = await _db.Products
            .Where(p => p.Category == product.Category && p.Id != product.Id && p.IsActive)
            .Take(3)
            .ToListAsync();

        ViewBag.RelatedProducts = relatedProducts;
        return View(product);
    }

    public async Task<IActionResult> Specifications()
    {
        var products = await _db.Products.Where(p => p.IsActive).OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
        return View(products);
    }

    public IActionResult ExportServices()
    {
        return View();
    }

    public IActionResult Quality()
    {
        return View();
    }

    public IActionResult Contact(int? productId, string? variety)
    {
        var model = new InquiryFormModel();
        if (!string.IsNullOrEmpty(variety))
        {
            model.RiceVariety = variety;
        }
        if (productId.HasValue)
        {
            model.ProductId = productId;
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitInquiry(InquiryFormModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please fill in all required fields accurately.";
            return View("Contact", model);
        }

        // Generate unique inquiry number
        var count = await _db.Inquiries.CountAsync();
        var inquiryNumber = $"INQ-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        // Check if customer exists
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email.ToLower() == model.Email.Trim().ToLower());

        var inquiry = new Inquiry
        {
            InquiryNumber = inquiryNumber,
            CustomerId = customer?.Id,
            FullName = model.FullName.Trim(),
            CompanyName = model.CompanyName?.Trim() ?? string.Empty,
            Email = model.Email.Trim(),
            Phone = model.Phone.Trim(),
            Country = model.Country.Trim(),
            RiceVariety = model.RiceVariety.Trim(),
            ProductId = model.ProductId,
            QuantityMT = model.QuantityMT,
            DestinationPort = model.DestinationPort.Trim(),
            IncoTerm = model.IncoTerm,
            PreferredPackaging = model.PreferredPackaging,
            Message = model.Message.Trim(),
            Status = InquiryStatus.New,
            Priority = model.QuantityMT >= 100 ? InquiryPriority.Urgent : InquiryPriority.Normal,
            CreatedAt = DateTime.UtcNow
        };

        _db.Inquiries.Add(inquiry);

        // Add Activity Log
        _db.ActivityLogs.Add(new ActivityLog
        {
            Action = "New Inquiry Received",
            Module = "Inquiry",
            Details = $"Inquiry {inquiryNumber} received from {model.FullName} ({model.Country}) for {model.QuantityMT} MT of {model.RiceVariety}.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Thank you, {model.FullName}! Your RFQ #{inquiryNumber} has been received. Our export trade desk will send an official quotation to {model.Email} within 24 hours.";
        return RedirectToAction(nameof(Contact));
    }

    public IActionResult DownloadBrochure()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", "ARROW_ARO_RICE_Export_Catalogue_2026.pdf");
        if (System.IO.File.Exists(filePath))
        {
            return PhysicalFile(filePath, "application/pdf", "ARROW_ARO_RICE_Export_Catalogue_2026.pdf");
        }

        // Return a generated export summary document if PDF not found
        var content = "ARROW ARO RICE - Premium Indian Basmati & Non-Basmati Rice Exporters\n" +
                      "Certified: APEDA, ISO 22000, HACCP, FSSAI, Halal, US FDA\n" +
                      "Export Office: Mundra Port & Mumbai | Processing: Taraori, Karnal, Haryana\n" +
                      "Contact: export@arrowarorice.com | WhatsApp: +91 98120 44550\n" +
                      "Website: https://www.arrowarorice.com";
        return File(System.Text.Encoding.UTF8.GetBytes(content), "text/plain", "ARROW_ARO_RICE_Company_Profile.txt");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
