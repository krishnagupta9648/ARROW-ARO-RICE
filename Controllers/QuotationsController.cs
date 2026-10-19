using ARROW_ARO_RICE.Data;
using ARROW_ARO_RICE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Controllers;

[Authorize(Roles = "Admin,Sales")]
public class QuotationsController : Controller
{
    private readonly ApplicationDbContext _db;

    public QuotationsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? status, string? search)
    {
        var query = _db.Quotations
            .Include(q => q.Customer)
            .Include(q => q.ConvertedSalesOrder)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<QuotationStatus>(status, out var sEnum))
        {
            query = query.Where(q => q.Status == sEnum);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(q => q.QuotationNumber.ToLower().Contains(s) ||
                                     q.CustomerName.ToLower().Contains(s) ||
                                     q.DestinationPort.ToLower().Contains(s));
        }

        var list = await query.OrderByDescending(q => q.QuotationDate).ToListAsync();
        ViewBag.SelectedStatus = status ?? "";
        ViewBag.SearchTerm = search ?? "";
        return View(list);
    }

    public async Task<IActionResult> Create(int? inquiryId, int? customerId)
    {
        ViewBag.Customers = await _db.Customers.OrderBy(c => c.CompanyName).ToListAsync();
        ViewBag.Products = await _db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();

        var quotation = new Quotation
        {
            QuotationDate = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddDays(15),
            PortOfLoading = "Mundra Port, Gujarat, India (INMUN1)",
            Currency = "USD",
            IncoTerm = "CIF",
            PaymentTerms = "30% Advance T/T, 70% against Scanned Bill of Lading",
            InspectionAgency = "SGS India pre-shipment container inspection at loading port",
            TermsAndConditions = "1. Packing in export seaworthy bags as specified.\n2. Shipment within 21 days from receipt of advance remittance.\n3. Weight & Quality certified by SGS / Bureau Veritas at load port.\n4. Contract governed by standard GAFTA trade terms.",
            BankDetails = "Beneficiary: ARROW ARO RICE EXPORTS PVT LTD\nBank: State Bank of India, Overseas Branch, Mumbai\nAccount No: 39182940192\nSWIFT: SBININBB104"
        };

        if (inquiryId.HasValue)
        {
            var inquiry = await _db.Inquiries.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == inquiryId.Value);
            if (inquiry != null)
            {
                quotation.InquiryId = inquiry.Id;
                quotation.CustomerName = inquiry.CompanyName.Length > 0 ? inquiry.CompanyName : inquiry.FullName;
                quotation.CustomerEmail = inquiry.Email;
                quotation.CustomerCountry = inquiry.Country;
                quotation.DestinationPort = inquiry.DestinationPort;
                quotation.IncoTerm = inquiry.IncoTerm;

                if (inquiry.CustomerId.HasValue)
                {
                    quotation.CustomerId = inquiry.CustomerId.Value;
                }

                if (inquiry.ProductId.HasValue && inquiry.Product != null)
                {
                    quotation.Items.Add(new QuotationItem
                    {
                        ProductId = inquiry.ProductId,
                        ProductName = inquiry.Product.Name,
                        SpecificationsSummary = $"{inquiry.Product.AverageLengthMm}mm AGL, {inquiry.Product.MoistureMaxPercent}% Moisture, {inquiry.Product.BrokenMaxPercent}% Broken max",
                        QuantityMT = inquiry.QuantityMT,
                        UnitPricePerMT = inquiry.Product.DefaultFobPricePerMT,
                        TotalAmount = inquiry.QuantityMT * inquiry.Product.DefaultFobPricePerMT,
                        Packaging = inquiry.PreferredPackaging
                    });
                }
            }
        }
        else if (customerId.HasValue)
        {
            var customer = await _db.Customers.FindAsync(customerId.Value);
            if (customer != null)
            {
                quotation.CustomerId = customer.Id;
                quotation.CustomerName = customer.CompanyName;
                quotation.CustomerEmail = customer.Email;
                quotation.CustomerCountry = customer.Country;
                quotation.DestinationPort = customer.PreferredDestinationPort ?? "";
                quotation.PaymentTerms = customer.PaymentTerms;
            }
        }

        return View(quotation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Quotation quotation, List<QuotationItem> items)
    {
        var count = await _db.Quotations.CountAsync();
        quotation.QuotationNumber = $"QT-{DateTime.UtcNow.Year}-{(count + 1):D3}";
        quotation.CreatedAt = DateTime.UtcNow;

        decimal subtotal = 0;
        foreach (var item in items)
        {
            if (item.QuantityMT > 0 && item.UnitPricePerMT > 0)
            {
                item.TotalAmount = item.QuantityMT * item.UnitPricePerMT;
                subtotal += item.TotalAmount;
                quotation.Items.Add(item);
            }
        }

        quotation.SubTotal = subtotal;
        quotation.GrandTotal = subtotal + quotation.FreightCharges + quotation.InsuranceCharges + quotation.OtherCharges - quotation.Discount;

        _db.Quotations.Add(quotation);

        // Update inquiry status if linked
        if (quotation.InquiryId.HasValue)
        {
            var inq = await _db.Inquiries.FindAsync(quotation.InquiryId.Value);
            if (inq != null)
            {
                inq.Status = InquiryStatus.QuotationSent;
            }
        }

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Quotation Created",
            Module = "Quotation",
            Details = $"Quotation {quotation.QuotationNumber} created for {quotation.CustomerName} ({quotation.GrandTotal:N0} {quotation.Currency}).",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Quotation {quotation.QuotationNumber} created successfully.";
        return RedirectToAction(nameof(Details), new { id = quotation.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var quotation = await _db.Quotations
            .Include(q => q.Customer)
            .Include(q => q.Items)
            .Include(q => q.ConvertedSalesOrder)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quotation == null) return NotFound();
        return View(quotation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, QuotationStatus status)
    {
        var quotation = await _db.Quotations.FindAsync(id);
        if (quotation == null) return NotFound();

        var old = quotation.Status;
        quotation.Status = status;

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Quotation Status Updated",
            Module = "Quotation",
            Details = $"Quotation {quotation.QuotationNumber} changed from {old} to {status}.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Quotation {quotation.QuotationNumber} marked as {status}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertToSalesOrder(int id)
    {
        var quotation = await _db.Quotations
            .Include(q => q.Items)
            .Include(q => q.Customer)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quotation == null) return NotFound();

        // Check if customer exists or create customer
        var customerId = quotation.CustomerId;
        if (!customerId.HasValue)
        {
            var customer = new Customer
            {
                CompanyName = quotation.CustomerName,
                ContactPerson = quotation.CustomerName,
                Email = quotation.CustomerEmail,
                Phone = "+00 000000",
                Country = quotation.CustomerCountry,
                PreferredDestinationPort = quotation.DestinationPort,
                Status = "Active Buyer",
                CreatedAt = DateTime.UtcNow
            };
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();
            customerId = customer.Id;
            quotation.CustomerId = customerId;
        }

        var orderCount = await _db.SalesOrders.CountAsync();
        var orderNumber = $"SO-{DateTime.UtcNow.Year}-{(orderCount + 1):D3}";

        var salesOrder = new SalesOrder
        {
            OrderNumber = orderNumber,
            QuotationId = quotation.Id,
            CustomerId = customerId.Value,
            OrderDate = DateTime.UtcNow,
            ExpectedShipmentDate = DateTime.UtcNow.AddDays(25),
            PortOfLoading = quotation.PortOfLoading,
            PortOfDestination = quotation.DestinationPort,
            IncoTerm = quotation.IncoTerm,
            PaymentTerms = quotation.PaymentTerms,
            Currency = quotation.Currency,
            TotalQuantityMT = quotation.Items.Sum(i => i.QuantityMT),
            TotalAmount = quotation.GrandTotal,
            Status = SalesOrderStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in quotation.Items)
        {
            salesOrder.Items.Add(new SalesOrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                QuantityMT = item.QuantityMT,
                UnitPricePerMT = item.UnitPricePerMT,
                TotalAmount = item.TotalAmount,
                Packaging = item.Packaging
            });
        }

        _db.SalesOrders.Add(salesOrder);
        quotation.Status = QuotationStatus.Accepted;

        // Auto-generate Proforma Commercial Invoice
        var invoiceCount = await _db.Invoices.CountAsync();
        var invoice = new Invoice
        {
            InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{(invoiceCount + 1):D3}",
            SalesOrder = salesOrder,
            CustomerId = customerId.Value,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Currency = quotation.Currency,
            SubTotal = quotation.SubTotal,
            GrandTotal = quotation.GrandTotal,
            PaidAmount = 0m,
            Status = "Unpaid",
            BankSwiftDetails = quotation.BankDetails,
            CreatedAt = DateTime.UtcNow
        };
        _db.Invoices.Add(invoice);

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Converted to Sales Order",
            Module = "SalesOrder",
            Details = $"Quotation {quotation.QuotationNumber} successfully converted to Sales Order {orderNumber}.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Quotation converted to Sales Order #{orderNumber}! Commercial Invoice #{invoice.InvoiceNumber} generated.";
        return RedirectToAction("Details", "SalesOrders", new { id = salesOrder.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var quotation = await _db.Quotations
            .Include(q => q.Items)
            .Include(q => q.ConvertedSalesOrder)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quotation == null) return NotFound();

        if (quotation.ConvertedSalesOrder != null)
        {
            TempData["ErrorMessage"] = $"Cannot delete quotation #{quotation.QuotationNumber} because it has already been converted into Sales Order #{quotation.ConvertedSalesOrder.OrderNumber}.";
            return RedirectToAction(nameof(Index));
        }

        _db.QuotationItems.RemoveRange(quotation.Items);
        _db.Quotations.Remove(quotation);

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Quotation Deleted",
            Module = "Quotation",
            Details = $"Quotation {quotation.QuotationNumber} deleted.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Quotation #{quotation.QuotationNumber} deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
