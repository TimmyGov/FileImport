using FileImportService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileImportService.Infrastructure.FileSystem;

/// <summary>
/// File archiver implementation
/// </summary>
public class FileArchiver : IFileArchiver
{
    private readonly ILogger<FileArchiver> _logger;

    public FileArchiver(ILogger<FileArchiver> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> ArchiveFileAsync(
        string sourceFilePath,
        string destinationFolder,
        bool success,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Ensure destination folder exists
                Directory.CreateDirectory(destinationFolder);

                var fileName = Path.GetFileName(sourceFilePath);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var destinationFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
                var destinationPath = Path.Combine(destinationFolder, destinationFileName);

                File.Move(sourceFilePath, destinationPath, overwrite: true);

                _logger.LogInformation(
                    "Archived file {FileName} to {DestinationPath} (Success: {Success})",
                    fileName,
                    destinationPath,
                    success);

                return destinationPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving file {SourceFilePath} to {DestinationFolder}",
                    sourceFilePath, destinationFolder);
                throw;
            }
        }, cancellationToken);
    }
}
