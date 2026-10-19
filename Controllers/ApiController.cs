using ARROW_ARO_RICE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ApiController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("dashboard/inquiries-by-country")]
    public async Task<IActionResult> GetInquiriesByCountry()
    {
        var data = await _db.Inquiries
            .GroupBy(i => i.Country)
            .Select(g => new { Country = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(6)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("dashboard/sales-trend")]
    public async Task<IActionResult> GetSalesTrend()
    {
        var orders = await _db.SalesOrders
            .Where(o => o.Status != Models.SalesOrderStatus.Cancelled)
            .ToListAsync();

        var trend = orders
            .GroupBy(o => o.OrderDate.ToString("MMM yyyy"))
            .Select(g => new { Month = g.Key, VolumeMT = g.Sum(x => x.TotalQuantityMT), RevenueUsd = g.Sum(x => x.TotalAmount) })
            .ToList();

        return Ok(trend);
    }

    [HttpGet("dashboard/variety-distribution")]
    public async Task<IActionResult> GetVarietyDistribution()
    {
        var items = await _db.SalesOrderItems
            .GroupBy(i => i.ProductName)
            .Select(g => new { Variety = g.Key, VolumeMT = g.Sum(x => x.QuantityMT) })
            .OrderByDescending(x => x.VolumeMT)
            .Take(5)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("track/{trackingNumber}")]
    public async Task<IActionResult> TrackShipment(string trackingNumber)
    {
        var q = trackingNumber.Trim().ToLower();
        var shipment = await _db.ExportShipments
            .Include(s => s.Documents)
            .FirstOrDefaultAsync(s => s.ShipmentNumber.ToLower() == q ||
                                      s.BillOfLadingNumber.ToLower() == q ||
                                      s.ContainerNumbers.ToLower().Contains(q) ||
                                      s.BookingNumber.ToLower() == q);

        if (shipment == null)
        {
            return NotFound(new { message = "No export shipment found for reference: " + trackingNumber });
        }

        return Ok(new
        {
            shipment.ShipmentNumber,
            shipment.ShippingLine,
            shipment.VesselName,
            shipment.VoyageNumber,
            shipment.ContainerNumbers,
            shipment.BillOfLadingNumber,
            shipment.PortOfLoading,
            shipment.PortOfDestination,
            EstimatedDeparture = shipment.EstimatedDeparture?.ToString("dd MMM yyyy"),
            EstimatedArrival = shipment.EstimatedArrival?.ToString("dd MMM yyyy"),
            Milestone = shipment.CurrentMilestone.ToString(),
            shipment.StatusDescription,
            shipment.TrackingNotes,
            Documents = shipment.Documents.Select(d => new { d.DocType, d.Title, d.DocumentNumber, d.IssuingAuthority })
        });
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(string? category)
    {
        var query = _db.Products.Where(p => p.IsActive).AsQueryable();
        if (!string.IsNullOrEmpty(category) && category != "All")
        {
            query = query.Where(p => p.Category == category);
        }

        var products = await query.Select(p => new
        {
            p.Id,
            p.Code,
            p.Name,
            p.Category,
            p.Variety,
            p.AverageLengthMm,
            p.MoistureMaxPercent,
            p.BrokenMaxPercent,
            p.DefaultFobPricePerMT,
            p.PackagingOptions,
            p.MinOrderQuantityMT
        }).ToListAsync();

        return Ok(products);
    }
}
