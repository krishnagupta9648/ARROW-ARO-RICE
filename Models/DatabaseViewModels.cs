namespace ARROW_ARO_RICE.Models;

public class DatabaseDashboardViewModel
{
    public string DatabaseProvider { get; set; } = "SQLite (Entity Framework Core)";
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseFilePath { get; set; } = string.Empty;
    public long DatabaseFileSizeBytes { get; set; }
    public DateTime? DatabaseLastModified { get; set; }
    public bool IsDatabaseOnline { get; set; } = true;
    public int TotalTablesCount => TableStats.Count;
    public int TotalRecordsCount => TableStats.Sum(t => t.RecordCount);

    public List<TableStatItem> TableStats { get; set; } = new();
    public List<DatabaseScriptItem> ScriptFiles { get; set; } = new();
}

public class TableStatItem
{
    public string TableName { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public string PrimaryKey { get; set; } = "Id (INT)";
}

public class DatabaseScriptItem
{
    public string FileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Description { get; set; } = string.Empty;
    public string SupportedEngines { get; set; } = "MySQL 8.0+, MariaDB, PostgreSQL, MS SQL Server, SQLite";
}

public class TableBrowserViewModel
{
    public string TableName { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public List<string> ColumnNames { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public string SearchQuery { get; set; } = string.Empty;
    public List<string> AvailableTables { get; set; } = new();
}

public class QueryResultViewModel
{
    public string SqlQuery { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long ExecutionTimeMs { get; set; }
    public List<string> ColumnNames { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
}
