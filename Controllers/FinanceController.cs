using ARROW_ARO_RICE.Data;
using ARROW_ARO_RICE.Models;
using ARROW_ARO_RICE.Models.ViewModels;
using ARROW_ARO_RICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Controllers;

[Authorize(Roles = "Admin,Accounts")]
public class FinanceController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IExportService _exportService;

    public FinanceController(ApplicationDbContext db, IExportService exportService)
    {
        _db = db;
        _exportService = exportService;
    }

    public async Task<IActionResult> Invoices(string? status, string? search)
    {
        var query = _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
            .Include(i => i.Payments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(i => i.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(i => i.InvoiceNumber.ToLower().Contains(s) ||
                                     i.Customer!.CompanyName.ToLower().Contains(s) ||
                                     i.SalesOrder!.OrderNumber.ToLower().Contains(s));
        }

        var list = await query.OrderByDescending(i => i.InvoiceDate).ToListAsync();
        ViewBag.SelectedStatus = status ?? "";
        ViewBag.SearchTerm = search ?? "";
        return View(list);
    }

    public async Task<IActionResult> InvoiceDetails(int id)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
                .ThenInclude(so => so!.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return NotFound();
        return View(invoice);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(int invoiceId, decimal amount, PaymentMethod paymentMethod, string transactionReference, string? bankName, string? notes)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null || amount <= 0)
        {
            TempData["ErrorMessage"] = "Invalid payment parameters.";
            return RedirectToAction(nameof(Invoices));
        }

        var count = await _db.PaymentReceipts.CountAsync();
        var receipt = new PaymentReceipt
        {
            ReceiptNumber = $"RCT-{DateTime.UtcNow.Year}-{(count + 1):D3}",
            InvoiceId = invoice.Id,
            CustomerId = invoice.CustomerId,
            PaymentDate = DateTime.UtcNow,
            Amount = amount,
            Currency = invoice.Currency,
            PaymentMethod = paymentMethod,
            TransactionReference = transactionReference,
            BankName = bankName,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        _db.PaymentReceipts.Add(receipt);
        invoice.PaidAmount += amount;

        if (invoice.PaidAmount >= invoice.GrandTotal)
        {
            invoice.Status = "Paid";
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = "PartiallyPaid";
        }

        _db.ActivityLogs.Add(new ActivityLog
        {
            UserName = User.Identity?.Name ?? "User",
            Action = "Payment Recorded",
            Module = "Finance",
            Details = $"Received {amount:N2} {invoice.Currency} for Invoice {invoice.InvoiceNumber} (Ref: {transactionReference}).",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Payment receipt {receipt.ReceiptNumber} recorded successfully.";
        return RedirectToAction(nameof(InvoiceDetails), new { id = invoiceId });
    }

    public async Task<IActionResult> Expenses(int? salesOrderId)
    {
        var query = _db.OrderExpenses
            .Include(e => e.SalesOrder)
                .ThenInclude(so => so!.Customer)
            .AsQueryable();

        if (salesOrderId.HasValue)
        {
            query = query.Where(e => e.SalesOrderId == salesOrderId.Value);
        }

        var list = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
        ViewBag.SalesOrders = await _db.SalesOrders.OrderByDescending(s => s.OrderDate).ToListAsync();
        ViewBag.SelectedSalesOrderId = salesOrderId;
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExpense(int salesOrderId, ExpenseCategory category, decimal amount, string vendorOrPayee, string? invoiceOrReceiptNumber, string? remarks)
    {
        var order = await _db.SalesOrders.FindAsync(salesOrderId);
        if (order == null || amount <= 0)
        {
            TempData["ErrorMessage"] = "Invalid order or expense amount.";
            return RedirectToAction(nameof(Expenses));
        }

        var expense = new OrderExpense
        {
            SalesOrderId = salesOrderId,
            Category = category,
            Amount = amount,
            Currency = "USD",
            VendorOrPayee = vendorOrPayee,
            InvoiceOrReceiptNumber = invoiceOrReceiptNumber,
            ExpenseDate = DateTime.UtcNow,
            Remarks = remarks,
            CreatedAt = DateTime.UtcNow
        };

        _db.OrderExpenses.Add(expense);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Expense of {amount:N2} USD recorded under {category}.";
        return RedirectToAction(nameof(Expenses), new { salesOrderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var expense = await _db.OrderExpenses.FindAsync(id);
        if (expense == null) return NotFound();

        var orderId = expense.SalesOrderId;
        _db.OrderExpenses.Remove(expense);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Expense record deleted successfully.";
        return RedirectToAction(nameof(Expenses), new { salesOrderId = orderId });
    }

    public async Task<IActionResult> Profitability()
    {
        var orders = await _db.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.Expenses)
            .Include(so => so.Items)
            .Where(so => so.Status != SalesOrderStatus.Cancelled)
            .ToListAsync();

        var vm = new ProfitabilityReportViewModel();

        foreach (var ord in orders)
        {
            // Estimate procurement cost based on items (avg procurement ~$850/MT for basmati or ~$450 for non-basmati)
            decimal estimatedProcurement = 0;
            foreach (var item in ord.Items)
            {
                var estUnitCost = item.UnitPricePerMT * 0.78m; // ~78% of sale price
                estimatedProcurement += item.QuantityMT * estUnitCost;
            }

            var logisticsAndExpenses = ord.Expenses.Sum(e => e.Amount);

            var itemVm = new ProfitabilityItem
            {
                OrderId = ord.Id,
                OrderNumber = ord.OrderNumber,
                CustomerName = ord.Customer?.CompanyName ?? "Unknown",
                Country = ord.Customer?.Country ?? "Unknown",
                QuantityMT = ord.TotalQuantityMT,
                RevenueUsd = ord.TotalAmount,
                ProcurementCostUsd = estimatedProcurement,
                LogisticsAndExpensesUsd = logisticsAndExpenses
            };

            vm.Orders.Add(itemVm);
            vm.TotalRevenueUsd += itemVm.RevenueUsd;
            vm.TotalProcurementCostUsd += itemVm.ProcurementCostUsd;
            vm.TotalLogisticsExpensesUsd += itemVm.LogisticsAndExpensesUsd;
            vm.TotalNetProfitUsd += itemVm.NetProfitUsd;
        }

        return View(vm);
    }

    public async Task<IActionResult> ExportInvoices()
    {
        var invoices = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        var headers = new[] { "Invoice No", "Date", "Due Date", "Order No", "Customer", "Currency", "Grand Total", "Paid", "Balance", "Status" };
        var bytes = _exportService.ExportToCsv(invoices, headers, i => new object?[]
        {
            i.InvoiceNumber,
            i.InvoiceDate.ToString("yyyy-MM-dd"),
            i.DueDate.ToString("yyyy-MM-dd"),
            i.SalesOrder?.OrderNumber,
            i.Customer?.CompanyName,
            i.Currency,
            i.GrandTotal,
            i.PaidAmount,
            i.BalanceAmount,
            i.Status
        });

        return File(bytes, "text/csv", $"ArrowAroRice_Invoices_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> ExportProfitability()
    {
        var orders = await _db.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.Expenses)
            .Include(so => so.Items)
            .Where(so => so.Status != SalesOrderStatus.Cancelled)
            .ToListAsync();

        var headers = new[] { "Order No", "Customer", "Country", "Quantity (MT)", "Revenue (USD)", "Procurement Cost (USD)", "Logistics & Expenses (USD)", "Net Margin (USD)", "Profit Margin (%)" };
        var bytes = _exportService.ExportToCsv(orders, headers, o =>
        {
            decimal procurement = o.Items.Sum(i => i.QuantityMT * (i.UnitPricePerMT * 0.78m));
            decimal expenses = o.Expenses.Sum(e => e.Amount);
            decimal net = o.TotalAmount - (procurement + expenses);
            decimal marginPct = o.TotalAmount > 0 ? (net / o.TotalAmount) * 100 : 0;

            return new object?[]
            {
                o.OrderNumber,
                o.Customer?.CompanyName,
                o.Customer?.Country,
                o.TotalQuantityMT,
                o.TotalAmount,
                procurement,
                expenses,
                net,
                Math.Round(marginPct, 1) + "%"
            };
        });

        return File(bytes, "text/csv", $"ArrowAroRice_Profitability_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
