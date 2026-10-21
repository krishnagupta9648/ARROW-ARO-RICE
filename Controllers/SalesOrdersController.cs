using ARROW_ARO_RICE.Data;
using ARROW_ARO_RICE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Controllers;

[Authorize(Roles = "Admin,Sales,Inventory,Accounts")]
public class SalesOrdersController : Controller
{
    private readonly ApplicationDbContext _db;

    public SalesOrdersController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? status, string? search)
    {
        var query = _db.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.Items)
            .Include(so => so.Shipments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<SalesOrderStatus>(status, out var sEnum))
        {
            query = query.Where(so => so.Status == sEnum);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(so => so.OrderNumber.ToLower().Contains(s) ||
                                      so.Customer!.CompanyName.ToLower().Contains(s) ||
                                      so.PortOfDestination.ToLower().Contains(s));
        }

        var list = await query.OrderByDescending(so => so.OrderDate).ToListAsync();
        ViewBag.SelectedStatus = status ?? "";
        ViewBag.SearchTerm = search ?? "";
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.Items)
                .ThenInclude(i => i.Product)
            .Include(so => so.Quotation)
            .Include(so => so.Shipments)
                .ThenInclude(s => s.Documents)
            .Include(so => so.Invoices)
                .ThenInclude(inv => inv.Payments)
            .Include(so => so.Expenses)
            .FirstOrDefaultAsync(so => so.Id == id);

        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, SalesOrderStatus status)
    {
        var order = await _db.SalesOrders.FindAsync(id);
        if (order == null) return NotFound();

        var old = order.Status;
        order.Status = status;

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Sales Order Status Updated",
            Module = "SalesOrder",
            Details = $"Sales Order {order.OrderNumber} status changed from {old} to {status}.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Sales Order {order.OrderNumber} status updated to {status}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> PurchaseOrders()
    {
        var pos = await _db.PurchaseOrders
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .Include(p => p.Product)
            .OrderByDescending(p => p.OrderDate)
            .ToListAsync();

        ViewBag.Suppliers = await _db.Suppliers.Where(s => s.IsActive).ToListAsync();
        ViewBag.Warehouses = await _db.Warehouses.Where(w => w.IsActive).ToListAsync();
        ViewBag.Products = await _db.Products.Where(p => p.IsActive).ToListAsync();

        return View(pos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePurchaseOrder(int supplierId, int warehouseId, int productId, decimal quantityMT, decimal unitRate, string? qualityRemarks)
    {
        var count = await _db.PurchaseOrders.CountAsync();
        var po = new PurchaseOrder
        {
            PoNumber = $"PO-{DateTime.UtcNow.Year}-{(count + 1):D3}",
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            ProductId = productId,
            QuantityMT = quantityMT,
            UnitRatePerMT = unitRate,
            TotalAmount = quantityMT * unitRate,
            Currency = "USD",
            Status = "Ordered",
            OrderDate = DateTime.UtcNow,
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(10),
            QualityCheckRemarks = qualityRemarks,
            CreatedAt = DateTime.UtcNow
        };

        _db.PurchaseOrders.Add(po);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Purchase Order {po.PoNumber} created successfully.";
        return RedirectToAction(nameof(PurchaseOrders));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSupplier(string name, string? contactPerson, string phone, string? email, string location, string supplierType, string? gstOrTaxNumber)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
        {
            TempData["ErrorMessage"] = "Supplier name and phone are required.";
            return RedirectToAction(nameof(PurchaseOrders));
        }

        var supplier = new Supplier
        {
            Name = name.Trim(),
            ContactPerson = contactPerson?.Trim() ?? string.Empty,
            Phone = phone.Trim(),
            Email = email?.Trim(),
            Location = location?.Trim() ?? "Haryana / Punjab",
            SupplierType = supplierType ?? "Mandi Agent",
            GstOrTaxNumber = gstOrTaxNumber?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Suppliers.Add(supplier);
        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Supplier Added",
            Module = "Procurement",
            Details = $"Added new agricultural supplier {supplier.Name} ({supplier.SupplierType}).",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Supplier '{supplier.Name}' registered successfully.";
        return RedirectToAction(nameof(PurchaseOrders));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _db.SalesOrders
            .Include(so => so.Shipments)
            .Include(so => so.Invoices)
            .Include(so => so.Expenses)
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == id);

        if (order == null) return NotFound();

        if (order.Shipments.Any() || order.Invoices.Any(i => i.PaidAmount > 0))
        {
            order.Status = SalesOrderStatus.Cancelled;
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Sales Order #{order.OrderNumber} has active shipments or paid invoices and has been marked as Cancelled.";
        }
        else
        {
            if (order.Expenses.Any()) _db.OrderExpenses.RemoveRange(order.Expenses);
            if (order.Invoices.Any()) _db.Invoices.RemoveRange(order.Invoices);
            if (order.Items.Any()) _db.SalesOrderItems.RemoveRange(order.Items);
            _db.SalesOrders.Remove(order);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Sales Order #{order.OrderNumber} was deleted successfully.";
        }

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Sales Order Deleted",
            Module = "SalesOrder",
            Details = $"Sales Order {order.OrderNumber} was deleted or cancelled.",
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePurchaseOrder(int id)
    {
        var po = await _db.PurchaseOrders.FindAsync(id);
        if (po == null) return NotFound();

        _db.PurchaseOrders.Remove(po);
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Purchase Order #{po.PoNumber} deleted.";
        return RedirectToAction(nameof(PurchaseOrders));
    }
}
