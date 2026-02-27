using BlazorFileManager.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace BlazorFileManager.Services;

/// <summary>
/// File storage service that stores files in the local filesystem
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileStorageService>? _logger;
    private readonly long _maxFileSize;

    /// <summary>
    /// Initializes a new instance of LocalFileStorageService
    /// </summary>
    /// <param name="basePath">Base directory path where files will be stored. If null, uses "uploads" folder in current directory.</param>
    /// <param name="logger">Optional logger</param>
    /// <param name="maxFileSize">Maximum file size in bytes (default: 10 MB)</param>
    public LocalFileStorageService(
        string? basePath = null, 
        ILogger<LocalFileStorageService>? logger = null,
        long maxFileSize = 10 * 1024 * 1024)
    {
        _basePath = basePath ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _logger = logger;
        _maxFileSize = maxFileSize;

        // Ensure the base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            _logger?.LogInformation("Created uploads directory at: {BasePath}", _basePath);
        }
    }

    /// <inheritdoc />
    public async Task<FileUploadItem> SaveFileAsync(IBrowserFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate unique filename to avoid conflicts
            var uniqueFileName = $"{Guid.NewGuid()}_{SanitizeFileName(file.Name)}";
            var filePath = Path.Combine(_basePath, uniqueFileName);

            _logger?.LogInformation("Saving file: {FileName} as {UniqueFileName}", file.Name, uniqueFileName);

            // Read and save the file
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.OpenReadStream(_maxFileSize, cancellationToken).CopyToAsync(fileStream, cancellationToken);

            _logger?.LogInformation("File saved successfully: {FilePath}", filePath);

            // Create FileUploadItem
            var fileInfo = new FileInfo(filePath);
            var fileItem = new FileUploadItem
            {
                FileName = file.Name,
                FileSize = fileInfo.Length,
                ContentType = file.ContentType,
                UploadDate = DateTime.Now,
                // Store the unique filename in FileData for later retrieval
                FileData = System.Text.Encoding.UTF8.GetBytes(uniqueFileName)
            };

            return fileItem;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving file: {FileName}", file.Name);
            throw new InvalidOperationException($"Failed to save file '{file.Name}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteFileAsync(FileUploadItem fileItem, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = GetFilePath(fileItem);
            
            if (!File.Exists(filePath))
            {
                _logger?.LogWarning("File not found for deletion: {FilePath}", filePath);
                return false;
            }

            await Task.Run(() => File.Delete(filePath), cancellationToken);
            _logger?.LogInformation("File deleted successfully: {FilePath}", filePath);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting file: {FileName}", fileItem.FileName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<Stream> GetFileStreamAsync(FileUploadItem fileItem, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = GetFilePath(fileItem);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {fileItem.FileName}");
            }

            var memoryStream = new MemoryStream();
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;
            
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error reading file: {FileName}", fileItem.FileName);
            throw new InvalidOperationException($"Failed to read file '{fileItem.FileName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> GetFileBytesAsync(FileUploadItem fileItem, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = GetFilePath(fileItem);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {fileItem.FileName}");
            }

            return await File.ReadAllBytesAsync(filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error reading file bytes: {FileName}", fileItem.FileName);
            throw new InvalidOperationException($"Failed to read file '{fileItem.FileName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Task<bool> FileExistsAsync(FileUploadItem fileItem, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = GetFilePath(fileItem);
            return Task.FromResult(File.Exists(filePath));
        }
        catch (Exception ex)
        {
            // If we can't get the file path (e.g., FileData contains actual file content instead of filename),
            // the file doesn't exist in storage - return false instead of throwing
            _logger?.LogDebug(ex, "File does not exist in storage (or cannot determine path): {FileName}", fileItem.FileName);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Gets the full file path from a FileUploadItem
    /// </summary>
    private string GetFilePath(FileUploadItem fileItem)
    {
        if (fileItem.FileData == null || fileItem.FileData.Length == 0)
        {
            throw new InvalidOperationException("FileData is empty. Cannot determine file path.");
        }

        // Check if FileData looks like a filename (small size, valid UTF8 string)
        // If FileData is larger than 1KB, it's probably the actual file content, not a filename
        if (fileItem.FileData.Length > 1024)
        {
            throw new InvalidOperationException($"FileData appears to contain file content (size: {fileItem.FileData.Length} bytes), not a filename. This file is not stored in the local storage service.");
        }

        try
        {
            var uniqueFileName = System.Text.Encoding.UTF8.GetString(fileItem.FileData);
            return Path.Combine(_basePath, uniqueFileName);
        }
        catch
        {
            throw new InvalidOperationException("FileData does not contain a valid filename.");
        }
    }

    /// <summary>
    /// Sanitizes a filename by removing invalid characters
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}
