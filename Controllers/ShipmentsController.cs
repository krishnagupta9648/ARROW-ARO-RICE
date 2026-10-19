using ARROW_ARO_RICE.Data;
using ARROW_ARO_RICE.Models;
using ARROW_ARO_RICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Controllers;

[Authorize(Roles = "Admin,Sales,Inventory")]
public class ShipmentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IExportService _exportService;

    public ShipmentsController(ApplicationDbContext db, IExportService exportService)
    {
        _db = db;
        _exportService = exportService;
    }

    public async Task<IActionResult> Index(string? milestone, string? search)
    {
        var query = _db.ExportShipments
            .Include(s => s.SalesOrder)
                .ThenInclude(so => so!.Customer)
            .Include(s => s.Documents)
            .AsQueryable();

        if (!string.IsNullOrEmpty(milestone) && Enum.TryParse<ShipmentMilestone>(milestone, out var mEnum))
        {
            query = query.Where(s => s.CurrentMilestone == mEnum);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(x => x.ShipmentNumber.ToLower().Contains(s) ||
                                     x.BookingNumber.ToLower().Contains(s) ||
                                     x.ContainerNumbers.ToLower().Contains(s) ||
                                     x.BillOfLadingNumber.ToLower().Contains(s) ||
                                     x.VesselName.ToLower().Contains(s) ||
                                     x.PortOfDestination.ToLower().Contains(s));
        }

        var list = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
        ViewBag.SelectedMilestone = milestone ?? "";
        ViewBag.SearchTerm = search ?? "";
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var shipment = await _db.ExportShipments
            .Include(s => s.SalesOrder)
                .ThenInclude(so => so!.Customer)
            .Include(s => s.SalesOrder)
                .ThenInclude(so => so!.Items)
            .Include(s => s.Documents)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shipment == null) return NotFound();
        return View(shipment);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int salesOrderId)
    {
        var salesOrder = await _db.SalesOrders
            .Include(so => so.Customer)
            .FirstOrDefaultAsync(so => so.Id == salesOrderId);

        if (salesOrder == null) return NotFound();

        var count = await _db.ExportShipments.CountAsync();
        var shipment = new ExportShipment
        {
            ShipmentNumber = $"SH-{DateTime.UtcNow.Year}-{(count + 1):D3}",
            SalesOrderId = salesOrder.Id,
            SalesOrder = salesOrder,
            PortOfLoading = salesOrder.PortOfLoading,
            PortOfDestination = salesOrder.PortOfDestination,
            ShippingLine = "Maersk Line",
            CurrentMilestone = ShipmentMilestone.OrderConfirmed,
            StatusDescription = "Shipment booking initiated with carrier",
            CreatedAt = DateTime.UtcNow
        };

        return View(shipment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExportShipment shipment)
    {
        shipment.CreatedAt = DateTime.UtcNow;
        _db.ExportShipments.Add(shipment);

        // Auto-generate draft export documents set
        shipment.Documents.Add(new ShipmentDocument
        {
            DocType = DocumentType.CommercialInvoice,
            Title = $"Commercial Invoice {shipment.ShipmentNumber}",
            DocumentNumber = $"INV-{DateTime.UtcNow.Year}-DRAFT",
            IssuedDate = DateTime.UtcNow,
            IssuingAuthority = "ARROW ARO RICE EXPORTS PVT LTD",
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        });

        shipment.Documents.Add(new ShipmentDocument
        {
            DocType = DocumentType.PackingList,
            Title = $"Export Packing List {shipment.ShipmentNumber}",
            DocumentNumber = $"PL-{DateTime.UtcNow.Year}-DRAFT",
            IssuedDate = DateTime.UtcNow,
            IssuingAuthority = "ARROW ARO RICE Logistics Division",
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        });

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Export Shipment Created",
            Module = "Shipment",
            Details = $"Shipment {shipment.ShipmentNumber} created for Sales Order #{shipment.SalesOrderId} to {shipment.PortOfDestination}.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Shipment {shipment.ShipmentNumber} initialized successfully.";
        return RedirectToAction(nameof(Details), new { id = shipment.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateMilestone(int id, ShipmentMilestone milestone, string statusDescription, string? notes)
    {
        var shipment = await _db.ExportShipments.FindAsync(id);
        if (shipment == null) return NotFound();

        var old = shipment.CurrentMilestone;
        shipment.CurrentMilestone = milestone;
        shipment.StatusDescription = statusDescription;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            shipment.TrackingNotes = (shipment.TrackingNotes + "\n[" + DateTime.UtcNow.ToString("dd MMM yyyy HH:mm") + "] " + notes).Trim();
        }

        if (milestone == ShipmentMilestone.VesselDeparted && !shipment.ActualDeparture.HasValue)
        {
            shipment.ActualDeparture = DateTime.UtcNow;
        }
        else if (milestone == ShipmentMilestone.Delivered && !shipment.ActualArrival.HasValue)
        {
            shipment.ActualArrival = DateTime.UtcNow;
        }

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Shipment Milestone Advanced",
            Module = "Shipment",
            Details = $"Shipment {shipment.ShipmentNumber} milestone advanced from {old} to {milestone}.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Shipment {shipment.ShipmentNumber} milestone updated to {milestone}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDocument(int shipmentId, DocumentType docType, string title, string documentNumber, string? issuingAuthority, string? remarks)
    {
        var shipment = await _db.ExportShipments.FindAsync(shipmentId);
        if (shipment == null) return NotFound();

        var doc = new ShipmentDocument
        {
            ExportShipmentId = shipmentId,
            DocType = docType,
            Title = title,
            DocumentNumber = documentNumber,
            IssuedDate = DateTime.UtcNow,
            IssuingAuthority = issuingAuthority ?? "Chamber of Commerce / APEDA",
            Remarks = remarks,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.ShipmentDocuments.Add(doc);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Document '{title}' added to export dossier.";
        return RedirectToAction(nameof(Details), new { id = shipmentId });
    }

    public async Task<IActionResult> Export()
    {
        var shipments = await _db.ExportShipments
            .Include(s => s.SalesOrder)
                .ThenInclude(so => so!.Customer)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var headers = new[] { "Shipment No", "Sales Order", "Buyer", "Shipping Line", "Vessel & Voyage", "Containers", "B/L No", "Port of Loading", "Port of Destination", "Milestone", "Status" };
        var bytes = _exportService.ExportToCsv(shipments, headers, s => new object?[]
        {
            s.ShipmentNumber,
            s.SalesOrder?.OrderNumber,
            s.SalesOrder?.Customer?.CompanyName,
            s.ShippingLine,
            $"{s.VesselName} {s.VoyageNumber}",
            s.ContainerNumbers,
            s.BillOfLadingNumber,
            s.PortOfLoading,
            s.PortOfDestination,
            s.CurrentMilestone.ToString(),
            s.StatusDescription
        });

        return File(bytes, "text/csv", $"ArrowAroRice_Shipments_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var shipment = await _db.ExportShipments
            .Include(s => s.Documents)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shipment == null) return NotFound();

        if (shipment.Documents.Any())
        {
            _db.ShipmentDocuments.RemoveRange(shipment.Documents);
        }

        _db.ExportShipments.Remove(shipment);

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Export Shipment Deleted",
            Module = "Shipment",
            Details = $"Shipment {shipment.ShipmentNumber} ({shipment.VesselName}) was deleted.",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Shipment #{shipment.ShipmentNumber} was deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument(int docId, int shipmentId)
    {
        var doc = await _db.ShipmentDocuments.FirstOrDefaultAsync(d => d.Id == docId && d.ExportShipmentId == shipmentId);
        if (doc == null) return NotFound();

        _db.ShipmentDocuments.Remove(doc);
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Document '{doc.Title}' was removed.";
        return RedirectToAction(nameof(Details), new { id = shipmentId });
    }
}
