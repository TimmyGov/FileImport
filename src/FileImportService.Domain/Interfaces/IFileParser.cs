using FileImportService.Domain.Models;

namespace FileImportService.Domain.Interfaces;

/// <summary>
/// Interface for file parsing implementations
/// </summary>
public interface IFileParser
{
    /// <summary>
    /// Parse file and extract data
    /// </summary>
    /// <param name="filePath">Path to file to parse</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File import result with parsed rows</returns>
    Task<FileImportResult> ParseAsync(string filePath, CancellationToken cancellationToken = default);
}
