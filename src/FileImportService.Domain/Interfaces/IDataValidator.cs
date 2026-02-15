using FileImportService.Domain.Models;

namespace FileImportService.Domain.Interfaces;

/// <summary>
/// Interface for data validation
/// </summary>
public interface IDataValidator
{
    /// <summary>
    /// Validate parsed data
    /// </summary>
    /// <param name="rows">Parsed rows to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateDataAsync(IEnumerable<ParsedRow> rows, CancellationToken cancellationToken = default);
}
