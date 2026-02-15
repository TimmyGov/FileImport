namespace FileImportService.Domain.Models;

/// <summary>
/// Individual parsed row from a file
/// </summary>
public class ParsedRow
{
    /// <summary>
    /// Row number in the source file
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Column values keyed by column name
    /// </summary>
    public Dictionary<string, string> Values { get; set; } = new();
}
