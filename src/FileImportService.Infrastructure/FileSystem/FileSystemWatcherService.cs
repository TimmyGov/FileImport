using FileImportService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileImportService.Infrastructure.FileSystem;

/// <summary>
/// File system watcher service implementation
/// </summary>
public class FileSystemWatcherService : IFileWatcher
{
    private readonly ILogger<FileSystemWatcherService> _logger;
    private FileSystemWatcher? _watcher;
    private Func<string, Task>? _fileCreatedCallback;

    public FileSystemWatcherService(ILogger<FileSystemWatcherService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void StartWatching(string directoryPath, Func<string, Task> fileCreatedCallback)
    {
        _fileCreatedCallback = fileCreatedCallback;

        // Ensure directory exists
        Directory.CreateDirectory(directoryPath);

        _watcher = new FileSystemWatcher(directoryPath)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*",
            EnableRaisingEvents = true
        };

        _watcher.Created += OnFileCreated;

        _logger.LogInformation("Started watching directory: {DirectoryPath}", directoryPath);
    }

    /// <inheritdoc/>
    public void StopWatching()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnFileCreated;
            _watcher.Dispose();
            _watcher = null;

            _logger.LogInformation("Stopped watching directory");
        }
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            _logger.LogInformation("File detected: {FilePath}", e.FullPath);

            // Wait a bit to ensure file is fully written
            await Task.Delay(1000);

            // Check if file still exists and is accessible
            if (File.Exists(e.FullPath) && IsFileReady(e.FullPath))
            {
                if (_fileCreatedCallback != null)
                {
                    await _fileCreatedCallback(e.FullPath);
                }
            }
            else
            {
                _logger.LogWarning("File no longer exists or is not ready: {FilePath}", e.FullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling file creation event for {FilePath}", e.FullPath);
        }
    }

    private bool IsFileReady(string filePath)
    {
        try
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
