namespace FileImportService.Domain.Interfaces;

/// <summary>
/// Interface for file system watching
/// </summary>
public interface IFileWatcher
{
    /// <summary>
    /// Start watching directory for new files
    /// </summary>
    /// <param name="directoryPath">Directory to watch</param>
    /// <param name="fileCreatedCallback">Callback when file is created</param>
    void StartWatching(string directoryPath, Func<string, Task> fileCreatedCallback);

    /// <summary>
    /// Stop watching directory
    /// </summary>
    void StopWatching();
}
