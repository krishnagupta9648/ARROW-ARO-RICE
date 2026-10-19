using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using ARROW_ARO_RICE.Data;
using ARROW_ARO_RICE.Models;
using ARROW_ARO_RICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Controllers;

[Authorize(Roles = "Admin")]
public class DatabaseController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly IExportService _exportService;

    private static readonly HashSet<string> ValidTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "Users", "Customers", "Inquiries", "Products", "Warehouses", 
        "InventoryStocks", "StockTransactions", "Suppliers", "PurchaseOrders", 
        "Quotations", "QuotationItems", "SalesOrders", "SalesOrderItems", 
        "ExportShipments", "ShipmentDocuments", "Invoices", "PaymentReceipts", 
        "OrderExpenses", "ActivityLogs"
    };

    public DatabaseController(
        ApplicationDbContext db, 
        IWebHostEnvironment env, 
        IConfiguration config,
        IExportService exportService)
    {
        _db = db;
        _env = env;
        _config = config;
        _exportService = exportService;
    }

    // GET: /Database
    public async Task<IActionResult> Index()
    {
        var dbPath = Path.Combine(_env.ContentRootPath, "arrow_rice.db");
        var dbFileInfo = new FileInfo(dbPath);

        var model = new DatabaseDashboardViewModel
        {
            DatabaseProvider = "SQLite 3 (Entity Framework Core 10.0)",
            ConnectionString = _config.GetConnectionString("DefaultConnection") ?? "Data Source=arrow_rice.db",
            DatabaseFilePath = dbFileInfo.Exists ? dbFileInfo.FullName : "Database file in memory / relative path",
            DatabaseFileSizeBytes = dbFileInfo.Exists ? dbFileInfo.Length : 0,
            DatabaseLastModified = dbFileInfo.Exists ? dbFileInfo.LastWriteTimeUtc : null,
            IsDatabaseOnline = await _db.Database.CanConnectAsync()
        };

        // Table stats
        model.TableStats = new List<TableStatItem>
        {
            new() { TableName = "Users", EntityName = "User", Category = "Authentication & Roles", Description = "Staff logins, BCrypt credentials, roles (Admin, Sales, Inventory, Accounts)", RecordCount = await _db.Users.CountAsync() },
            new() { TableName = "Customers", EntityName = "Customer", Category = "CRM & Trade Partners", Description = "International grain importers, VAT/Tax IDs, credit limits & payment terms", RecordCount = await _db.Customers.CountAsync() },
            new() { TableName = "Inquiries", EntityName = "Inquiry", Category = "CRM & Trade Partners", Description = "Buyer RFQ leads, packaging requirements, target CIF ports & follow-up dates", RecordCount = await _db.Inquiries.CountAsync() },
            new() { TableName = "Products", EntityName = "Product", Category = "Grain Catalog", Description = "Rice varieties (1121, 1509, Traditional, Sona Masoori, IR64), specs & moisture %", RecordCount = await _db.Products.CountAsync() },
            new() { TableName = "Warehouses", EntityName = "Warehouse", Category = "Logistics & Milling", Description = "Milling complexes and port CFS hubs (Karnal, Mundra, Nhava Sheva JNPT)", RecordCount = await _db.Warehouses.CountAsync() },
            new() { TableName = "InventoryStocks", EntityName = "InventoryStock", Category = "Logistics & Milling", Description = "Real-time stock lots by warehouse, batch codes, reserved metric tons", RecordCount = await _db.InventoryStocks.CountAsync() },
            new() { TableName = "StockTransactions", EntityName = "StockTransaction", Category = "Logistics & Milling", Description = "Audit trail of inward procurement, outward export allocations, and adjustments", RecordCount = await _db.StockTransactions.CountAsync() },
            new() { TableName = "Suppliers", EntityName = "Supplier", Category = "Procurement", Description = "Agricultural mandis, farmer cooperative federations & packaging vendors", RecordCount = await _db.Suppliers.CountAsync() },
            new() { TableName = "PurchaseOrders", EntityName = "PurchaseOrder", Category = "Procurement", Description = "Grain procurement contracts, delivery status & quality check remarks", RecordCount = await _db.PurchaseOrders.CountAsync() },
            new() { TableName = "Quotations", EntityName = "Quotation", Category = "Export Sales", Description = "International proforma quotes with IncoTerms, payment terms & bank details", RecordCount = await _db.Quotations.CountAsync() },
            new() { TableName = "QuotationItems", EntityName = "QuotationItem", Category = "Export Sales", Description = "Line items for quotes: rice variety, metric tons, unit rate & packaging", RecordCount = await _db.QuotationItems.CountAsync() },
            new() { TableName = "SalesOrders", EntityName = "SalesOrder", Category = "Export Sales", Description = "Confirmed export contracts, buyer PO references & shipping deadlines", RecordCount = await _db.SalesOrders.CountAsync() },
            new() { TableName = "SalesOrderItems", EntityName = "SalesOrderItem", Category = "Export Sales", Description = "Contract line items, metric tons dispatched & total values", RecordCount = await _db.SalesOrderItems.CountAsync() },
            new() { TableName = "ExportShipments", EntityName = "ExportShipment", Category = "Shipping & Logistics", Description = "10-stage ocean voyage tracking, vessel names, container numbers & ETAs", RecordCount = await _db.ExportShipments.CountAsync() },
            new() { TableName = "ShipmentDocuments", EntityName = "ShipmentDocument", Category = "Shipping & Logistics", Description = "Bills of Lading, Phytosanitary, Certificate of Origin & SGS test reports", RecordCount = await _db.ShipmentDocuments.CountAsync() },
            new() { TableName = "Invoices", EntityName = "Invoice", Category = "Finance & Accounts", Description = "Commercial export invoices, due dates, paid balances & payment status", RecordCount = await _db.Invoices.CountAsync() },
            new() { TableName = "PaymentReceipts", EntityName = "PaymentReceipt", Category = "Finance & Accounts", Description = "Wire transfer remittances, Swift MT103 confirmations & bank references", RecordCount = await _db.PaymentReceipts.CountAsync() },
            new() { TableName = "OrderExpenses", EntityName = "OrderExpense", Category = "Finance & Accounts", Description = "Order-wise ocean freight, THC port charges, SGS inspection & custom duties", RecordCount = await _db.OrderExpenses.CountAsync() },
            new() { TableName = "ActivityLogs", EntityName = "ActivityLog", Category = "Audit Trail", Description = "System audit records, user logins, data changes and IP timestamps", RecordCount = await _db.ActivityLogs.CountAsync() }
        };

        // Standalone SQL scripts
        var schemaPath = Path.Combine(_env.ContentRootPath, "database", "schema.sql");
        var seedPath = Path.Combine(_env.ContentRootPath, "database", "seed_data.sql");

        var schemaInfo = new FileInfo(schemaPath);
        var seedInfo = new FileInfo(seedPath);

        model.ScriptFiles = new List<DatabaseScriptItem>
        {
            new()
            {
                FileName = "schema.sql",
                RelativePath = "database/schema.sql",
                FileSizeBytes = schemaInfo.Exists ? schemaInfo.Length : 0,
                Description = "Complete Data Definition Language (DDL) with 19 tables, indexes, constraints, and foreign key relations. Compatible with MySQL, MariaDB, PostgreSQL, MS SQL Server, and SQLite."
            },
            new()
            {
                FileName = "seed_data.sql",
                RelativePath = "database/seed_data.sql",
                FileSizeBytes = seedInfo.Exists ? seedInfo.Length : 0,
                Description = "Realistic enterprise dataset (DML) including staff users, international buyers, full rice specifications, warehouse inventory lots, active quotations, sales orders, ocean container shipments, and financial ledgers."
            }
        };

        return View(model);
    }

    // GET: /Database/Table?name=Customers
    public async Task<IActionResult> Table(string name = "Products", string search = "")
    {
        if (string.IsNullOrWhiteSpace(name) || !ValidTables.Contains(name))
        {
            name = "Products";
        }

        var model = new TableBrowserViewModel
        {
            TableName = name,
            SearchQuery = search,
            AvailableTables = ValidTables.OrderBy(t => t).ToList()
        };

        var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM \"{name}\" LIMIT 100;";

        using var reader = await command.ExecuteReaderAsync();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            model.ColumnNames.Add(reader.GetName(i));
        }

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var colName = reader.GetName(i);
                var val = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[colName] = val;
            }

            // Search filter in memory if provided
            if (string.IsNullOrWhiteSpace(search) || 
                row.Values.Any(v => v?.ToString()?.Contains(search, StringComparison.OrdinalIgnoreCase) == true))
            {
                model.Rows.Add(row);
            }
        }

        model.TotalCount = model.Rows.Count;
        return View(model);
    }

    // POST: /Database/ExecuteQuery
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExecuteQuery(string sqlQuery)
    {
        var model = new QueryResultViewModel
        {
            SqlQuery = sqlQuery ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(sqlQuery))
        {
            model.Success = false;
            model.ErrorMessage = "Please enter a valid SQL query.";
            return PartialView("_QueryResultPartial", model);
        }

        var trimmed = sqlQuery.Trim();
        var blockedKeywords = new[] { "DROP", "DELETE", "TRUNCATE", "UPDATE", "INSERT", "ALTER", "CREATE", "GRANT", "REVOKE" };
        
        foreach (var keyword in blockedKeywords)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, $@"\b{keyword}\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                model.Success = false;
                model.ErrorMessage = $"Execution blocked: '{keyword}' statements are disallowed in the read-only SQL studio for data safety. Only SELECT, PRAGMA, or EXPLAIN queries are permitted.";
                return PartialView("_QueryResultPartial", model);
            }
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var connection = _db.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var command = connection.CreateCommand();
            command.CommandText = trimmed;

            using var reader = await command.ExecuteReaderAsync();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                model.ColumnNames.Add(reader.GetName(i));
            }

            int count = 0;
            while (await reader.ReadAsync() && count < 200)
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                model.Rows.Add(row);
                count++;
            }

            stopwatch.Stop();
            model.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            model.Success = true;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            model.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            model.Success = false;
            model.ErrorMessage = ex.Message;
        }

        return PartialView("_QueryResultPartial", model);
    }

    // GET: /Database/DownloadSqlite
    public IActionResult DownloadSqlite()
    {
        var dbPath = Path.Combine(_env.ContentRootPath, "arrow_rice.db");
        if (!System.IO.File.Exists(dbPath))
        {
            return NotFound("SQLite database file not found.");
        }

        var fileName = $"arrow_rice_backup_{DateTime.UtcNow:yyyyMMdd_HHmm}.db";
        return PhysicalFile(dbPath, "application/x-sqlite3", fileName);
    }

    // GET: /Database/DownloadSchema
    public IActionResult DownloadSchema()
    {
        var path = Path.Combine(_env.ContentRootPath, "database", "schema.sql");
        if (!System.IO.File.Exists(path))
        {
            return NotFound("schema.sql file not found.");
        }

        return PhysicalFile(path, "application/sql", "arrow_aro_rice_schema.sql");
    }

    // GET: /Database/DownloadSeedData
    public IActionResult DownloadSeedData()
    {
        var path = Path.Combine(_env.ContentRootPath, "database", "seed_data.sql");
        if (!System.IO.File.Exists(path))
        {
            return NotFound("seed_data.sql file not found.");
        }

        return PhysicalFile(path, "application/sql", "arrow_aro_rice_seed_data.sql");
    }

    // GET: /Database/ExportTableCsv?name=Products
    public async Task<IActionResult> ExportTableCsv(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || !ValidTables.Contains(name))
        {
            return BadRequest("Invalid table name specified.");
        }

        var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM \"{name}\";";

        using var reader = await command.ExecuteReaderAsync();
        var columns = new List<string>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            columns.Add(reader.GetName(i));
        }

        var rows = new List<string[]>();
        while (await reader.ReadAsync())
        {
            var rowValues = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                rowValues[i] = reader.IsDBNull(i) ? "" : reader.GetValue(i)?.ToString() ?? "";
            }
            rows.Add(rowValues);
        }

        var sb = new StringBuilder();
        sb.Append('\uFEFF');
        sb.AppendLine(string.Join(",", columns.Select(c => $"\"{c.Replace("\"", "\"\"")}\"")));

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", row.Select(val => $"\"{val.Replace("\"", "\"\"")}\"")));
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv; charset=utf-8", $"{name}_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }

    // POST: /Database/ResetDemoData
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetDemoData()
    {
        try
        {
            // Re-run initializer
            await DbInitializer.InitializeAsync(_db);

            _db.ActivityLogs.Add(new ActivityLog
            {
                Action = "Database Verification",
                Module = "Database",
                Details = "Admin triggered database health check / verification initialization.",
                UserName = User.Identity?.Name ?? "Admin",
                Timestamp = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Database integrity check completed successfully. All 19 relational tables and seed data are verified.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Database verification encountered an issue: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
