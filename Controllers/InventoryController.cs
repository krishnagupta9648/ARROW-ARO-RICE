using ARROW_ARO_RICE.Data;
using ARROW_ARO_RICE.Models;
using ARROW_ARO_RICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Controllers;

[Authorize(Roles = "Admin,Inventory")]
public class InventoryController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IExportService _exportService;

    public InventoryController(ApplicationDbContext db, IExportService exportService)
    {
        _db = db;
        _exportService = exportService;
    }

    public async Task<IActionResult> Index()
    {
        var warehouses = await _db.Warehouses
            .Include(w => w.Stocks)
                .ThenInclude(s => s.Product)
            .ToListAsync();

        var products = await _db.Products.Where(p => p.IsActive).ToListAsync();

        ViewBag.Warehouses = warehouses;
        ViewBag.Products = products;

        var allStocks = await _db.InventoryStocks
            .Include(s => s.Warehouse)
            .Include(s => s.Product)
            .OrderBy(s => s.Warehouse!.Name)
            .ThenBy(s => s.Product!.Name)
            .ToListAsync();

        return View(allStocks);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StockIn(int warehouseId, int productId, decimal quantityMT, string batchNumber, decimal moisture, string packagingType, string? notes)
    {
        var warehouse = await _db.Warehouses.FindAsync(warehouseId);
        var product = await _db.Products.FindAsync(productId);
        if (warehouse == null || product == null || quantityMT <= 0)
        {
            TempData["ErrorMessage"] = "Invalid warehouse or product specification.";
            return RedirectToAction(nameof(Index));
        }

        var stock = await _db.InventoryStocks
            .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId && s.BatchNumber == batchNumber);

        if (stock == null)
        {
            stock = new InventoryStock
            {
                WarehouseId = warehouseId,
                ProductId = productId,
                QuantityMT = quantityMT,
                ReservedMT = 0,
                BatchNumber = string.IsNullOrWhiteSpace(batchNumber) ? $"BATCH-{DateTime.UtcNow:yyyyMMdd}-{productId}" : batchNumber,
                MoistureTestedPercent = moisture,
                PackagingType = packagingType,
                ReorderLevelMT = 50.0m,
                LastUpdated = DateTime.UtcNow
            };
            _db.InventoryStocks.Add(stock);
        }
        else
        {
            stock.QuantityMT += quantityMT;
            stock.MoistureTestedPercent = moisture;
            stock.LastUpdated = DateTime.UtcNow;
        }

        // Add transaction ledger
        _db.StockTransactions.Add(new StockTransaction
        {
            WarehouseId = warehouseId,
            ProductId = productId,
            TransactionType = StockTransactionType.StockIn,
            QuantityMT = quantityMT,
            BatchNumber = stock.BatchNumber,
            ReferenceDocument = "INWARD-RECEIPT",
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        });

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Stock Inward Recorded",
            Module = "Inventory",
            Details = $"Received {quantityMT} MT of {product.Name} into {warehouse.Name} (Batch: {stock.BatchNumber}).",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Successfully added {quantityMT} MT to inventory.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StockOut(int stockId, decimal quantityMT, string? reference, string? notes)
    {
        var stock = await _db.InventoryStocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == stockId);

        if (stock == null || quantityMT <= 0 || stock.QuantityMT < quantityMT)
        {
            TempData["ErrorMessage"] = "Insufficient stock quantity available for dispatch.";
            return RedirectToAction(nameof(Index));
        }

        stock.QuantityMT -= quantityMT;
        stock.LastUpdated = DateTime.UtcNow;

        _db.StockTransactions.Add(new StockTransaction
        {
            WarehouseId = stock.WarehouseId,
            ProductId = stock.ProductId,
            TransactionType = StockTransactionType.StockOut,
            QuantityMT = quantityMT,
            BatchNumber = stock.BatchNumber,
            ReferenceDocument = reference ?? "DISPATCH",
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        });

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Stock Outward Dispatched",
            Module = "Inventory",
            Details = $"Dispatched {quantityMT} MT of {stock.Product?.Name} from {stock.Warehouse?.Name} (Ref: {reference}).",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Successfully dispatched {quantityMT} MT.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Transactions(int? warehouseId, int? productId)
    {
        var query = _db.StockTransactions
            .Include(t => t.Warehouse)
            .Include(t => t.Product)
            .AsQueryable();

        if (warehouseId.HasValue) query = query.Where(t => t.WarehouseId == warehouseId.Value);
        if (productId.HasValue) query = query.Where(t => t.ProductId == productId.Value);

        var list = await query.OrderByDescending(t => t.CreatedAt).Take(100).ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWarehouse(string name, string? code, string location, string city, string state, decimal capacityMT, string? contactPerson, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "Warehouse name is required.";
            return RedirectToAction(nameof(Index));
        }

        var warehouse = new Warehouse
        {
            Name = name.Trim(),
            Code = string.IsNullOrWhiteSpace(code) ? $"WH-{DateTime.UtcNow:yyMM}-{Guid.NewGuid().ToString()[..4].ToUpper()}" : code.Trim().ToUpper(),
            Location = location ?? "Industrial Area",
            City = string.IsNullOrWhiteSpace(city) ? "Karnal" : city.Trim(),
            State = string.IsNullOrWhiteSpace(state) ? "Haryana" : state.Trim(),
            CapacityMT = capacityMT > 0 ? capacityMT : 5000,
            ContactPerson = contactPerson,
            Phone = phone,
            IsActive = true
        };

        _db.Warehouses.Add(warehouse);
        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Warehouse Created",
            Module = "Inventory",
            Details = $"Created warehouse facility {warehouse.Name} ({warehouse.Code}) with capacity {warehouse.CapacityMT} MT.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Warehouse '{warehouse.Name}' added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWarehouse(int id)
    {
        var warehouse = await _db.Warehouses
            .Include(w => w.Stocks)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (warehouse == null) return NotFound();

        if (warehouse.Stocks != null && warehouse.Stocks.Any(s => s.QuantityMT > 0))
        {
            TempData["ErrorMessage"] = "Cannot delete warehouse facility while it holds active physical rice stocks. Please dispatch or transfer stocks first.";
            return RedirectToAction(nameof(Index));
        }

        if (warehouse.Stocks != null && warehouse.Stocks.Any())
        {
            _db.InventoryStocks.RemoveRange(warehouse.Stocks);
        }

        _db.Warehouses.Remove(warehouse);
        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Warehouse Deleted",
            Module = "Inventory",
            Details = $"Deleted warehouse facility: {warehouse.Name} ({warehouse.Code})",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Warehouse '{warehouse.Name}' removed successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStock(int id)
    {
        var stock = await _db.InventoryStocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (stock == null) return NotFound();

        if (stock.ReservedMT > 0)
        {
            TempData["ErrorMessage"] = $"Cannot remove stock batch '{stock.BatchNumber}' because {stock.ReservedMT} MT is currently reserved for export orders.";
            return RedirectToAction(nameof(Index));
        }

        var batch = stock.BatchNumber;
        var prodName = stock.Product?.Name;
        _db.InventoryStocks.Remove(stock);

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Stock Batch Deleted",
            Module = "Inventory",
            Details = $"Deleted inventory batch {batch} ({prodName})",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Stock batch '{batch}' removed successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Export()
    {
        var stocks = await _db.InventoryStocks
            .Include(s => s.Warehouse)
            .Include(s => s.Product)
            .OrderBy(s => s.Warehouse!.Name)
            .ToListAsync();

        var headers = new[] { "Warehouse", "Product Code", "Product Variety", "Batch Number", "Stock (MT)", "Reserved (MT)", "Available (MT)", "Moisture (%)", "Packaging", "Last Updated" };
        var bytes = _exportService.ExportToCsv(stocks, headers, s => new object?[]
        {
            s.Warehouse?.Name,
            s.Product?.Code,
            s.Product?.Name,
            s.BatchNumber,
            s.QuantityMT,
            s.ReservedMT,
            s.AvailableMT,
            s.MoistureTestedPercent,
            s.PackagingType,
            s.LastUpdated.ToString("yyyy-MM-dd HH:mm")
        });

        return File(bytes, "text/csv", $"ArrowAroRice_Inventory_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
