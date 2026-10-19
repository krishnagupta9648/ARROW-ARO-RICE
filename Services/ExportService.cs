using System.Text;

namespace ARROW_ARO_RICE.Services;

public interface IExportService
{
    byte[] ExportToCsv<T>(IEnumerable<T> data, string[] headers, Func<T, object?[]> rowSelector);
}

public class ExportService : IExportService
{
    public byte[] ExportToCsv<T>(IEnumerable<T> data, string[] headers, Func<T, object?[]> rowSelector)
    {
        var sb = new StringBuilder();

        // Write UTF-8 BOM so Excel opens it with correct characters
        sb.Append('\uFEFF');

        // Write header
        sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

        // Write rows
        foreach (var item in data)
        {
            var values = rowSelector(item);
            sb.AppendLine(string.Join(",", values.Select(v => EscapeCsv(v?.ToString() ?? ""))));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
