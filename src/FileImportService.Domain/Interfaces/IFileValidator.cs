using FileImportService.Domain.Models;

namespace FileImportService.Domain.Interfaces;

/// <summary>
/// Interface for file validation
/// </summary>
public interface IFileValidator
{
    /// <summary>
    /// Validate file before processing
    /// </summary>
    /// <param name="filePath">Path to file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateFileAsync(string filePath, CancellationToken cancellationToken = default);
}
