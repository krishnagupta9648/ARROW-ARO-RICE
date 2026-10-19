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

    public async Task<IActionResult> Index()
    {
        var products = await _db.Products
            .Include(p => p.InventoryStocks)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return View(products);
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
}
