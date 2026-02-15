namespace FileImportService.Application.Configuration;

/// <summary>
/// Column definition for fixed-length file format
/// </summary>
public class FixedLengthColumnDefinition
{
    /// <summary>
    /// Column name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Start position (0-based)
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// Column length
    /// </summary>
    public int Length { get; set; }
}
