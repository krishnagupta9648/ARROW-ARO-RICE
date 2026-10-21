using ARROW_ARO_RICE.Data;
using ARROW_ARO_RICE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Controllers;

[Authorize(Roles = "Admin,Inventory,Sales")]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProductsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? status)
    {
        var allProducts = await _db.Products
            .Include(p => p.InventoryStocks)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        ViewBag.TotalCount = allProducts.Count;
        ViewBag.ActiveCount = allProducts.Count(p => p.IsActive);
        ViewBag.ArchivedCount = allProducts.Count(p => !p.IsActive);
        ViewBag.CurrentStatus = status ?? "all";

        var filtered = status?.ToLower() switch
        {
            "active" => allProducts.Where(p => p.IsActive).ToList(),
            "archived" => allProducts.Where(p => !p.IsActive).ToList(),
            _ => allProducts
        };

        return View(filtered);
    }

    public IActionResult Create()
    {
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        product.CreatedAt = DateTime.UtcNow;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Product {product.Name} added successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product model)
    {
        if (id != model.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.Code = model.Code;
        product.Name = model.Name;
        product.Category = model.Category;
        product.Variety = model.Variety;
        product.ProcessingType = model.ProcessingType;
        product.AverageLengthMm = model.AverageLengthMm;
        product.MoistureMaxPercent = model.MoistureMaxPercent;
        product.BrokenMaxPercent = model.BrokenMaxPercent;
        product.ChalkyMaxPercent = model.ChalkyMaxPercent;
        product.ForeignMatterMaxPercent = model.ForeignMatterMaxPercent;
        product.AdmixtureMaxPercent = model.AdmixtureMaxPercent;
        product.ElongationRatio = model.ElongationRatio;
        product.AromaGrade = model.AromaGrade;
        product.KettWhiteness = model.KettWhiteness;
        product.PackagingOptions = model.PackagingOptions;
        product.MinOrderQuantityMT = model.MinOrderQuantityMT;
        product.DefaultFobPricePerMT = model.DefaultFobPricePerMT;
        product.Currency = model.Currency;
        product.Description = model.Description;
        product.Highlights = model.Highlights;
        product.IsActive = model.IsActive;
        product.IsFeatured = model.IsFeatured;

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Product {product.Name} updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.IsActive = !product.IsActive;
        if (!product.IsActive)
        {
            product.IsFeatured = false;
        }
        await _db.SaveChangesAsync();

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "Admin",
            Action = product.IsActive ? "Product Activated" : "Product Archived",
            Module = "Product",
            Details = $"Product {product.Name} ({product.Code}) marked as {(product.IsActive ? "Active" : "Archived")}.",
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Product '{product.Name}' is now {(product.IsActive ? "Active" : "Archived")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFeatured(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.IsFeatured = !product.IsFeatured;
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Product '{product.Name}' featured status set to {(product.IsFeatured ? "Featured on Website" : "Standard")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products
            .Include(p => p.InventoryStocks)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();

        // Check if referenced in trade orders, quotes, POs, or stock transactions
        var inQuotes = await _db.QuotationItems.AnyAsync(qi => qi.ProductId == id);
        var inOrders = await _db.SalesOrderItems.AnyAsync(soi => soi.ProductId == id);
        var inPurchaseOrders = await _db.PurchaseOrders.AnyAsync(po => po.ProductId == id);
        var inTransactions = await _db.StockTransactions.AnyAsync(st => st.ProductId == id);

        var hasTradeHistory = inQuotes || inOrders || inPurchaseOrders || inTransactions;

        if (hasTradeHistory)
        {
            if (product.IsActive)
            {
                // Soft delete by deactivating to preserve audit and transaction history
                product.IsActive = false;
                product.IsFeatured = false;
                await _db.SaveChangesAsync();

                _db.ActivityLogs.Add(new ActivityLog
                {
                    UserName = User.Identity?.Name ?? "Admin",
                    Action = "Product Archived",
                    Module = "Product",
                    Details = $"Product {product.Name} ({product.Code}) archived due to linked trade records.",
                    Timestamp = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Product '{product.Name}' is referenced in official trade records/orders and has been deactivated and archived (hidden from public catalog).";
            }
            else
            {
                TempData["ErrorMessage"] = $"Product '{product.Name}' cannot be permanently erased because historical sales orders or purchase contracts reference it. It remains safely archived.";
            }
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Safely unlink from Inquiries so buyer inquiry leads remain intact without FK violations
            var linkedInquiries = await _db.Inquiries.Where(i => i.ProductId == id).ToListAsync();
            foreach (var inq in linkedInquiries)
            {
                inq.ProductId = null;
            }

            // Remove associated warehouse inventory stock rows for this product
            var stocks = await _db.InventoryStocks.Where(s => s.ProductId == id).ToListAsync();
            if (stocks.Any())
            {
                _db.InventoryStocks.RemoveRange(stocks);
            }

            var productName = product.Name;
            var productCode = product.Code;

            _db.Products.Remove(product);

            _db.ActivityLogs.Add(new ActivityLog
            {
                UserName = User.Identity?.Name ?? "Admin",
                Action = "Product Deleted",
                Module = "Product",
                Details = $"Product {productName} ({productCode}) was permanently deleted.",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Product '{productName}' was permanently deleted.";
        }
        catch (DbUpdateException)
        {
            // Robust fallback: If any database-level constraint prevents removal, safely soft-delete / archive
            _db.ChangeTracker.Clear();
            var fallback = await _db.Products.FindAsync(id);
            if (fallback != null)
            {
                fallback.IsActive = false;
                fallback.IsFeatured = false;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Product '{fallback.Name}' has related business records and has been archived and hidden from public catalog.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not delete product due to database constraints.";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
