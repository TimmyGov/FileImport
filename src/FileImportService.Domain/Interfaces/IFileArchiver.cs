namespace FileImportService.Domain.Interfaces;

/// <summary>
/// Interface for file archiving
/// </summary>
public interface IFileArchiver
{
    /// <summary>
    /// Archive processed file to destination folder
    /// </summary>
    /// <param name="sourceFilePath">Source file path</param>
    /// <param name="destinationFolder">Destination folder</param>
    /// <param name="success">Whether processing was successful</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Destination file path</returns>
    Task<string> ArchiveFileAsync(
        string sourceFilePath,
        string destinationFolder,
        bool success,
        CancellationToken cancellationToken = default);
}
