using BlazorFileManager.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Concurrent;

namespace BlazorFileManager.Services;

/// <summary>
/// File storage service that stores files in memory.
/// Useful for testing or temporary file storage. Files are lost when the application restarts.
/// </summary>
public class InMemoryFileStorageService : IFileStorageService
{
    private readonly ConcurrentDictionary<string, byte[]> _fileStorage = new();
    private readonly long _maxFileSize;

    /// <summary>
    /// Initializes a new instance of InMemoryFileStorageService
    /// </summary>
    /// <param name="maxFileSize">Maximum file size in bytes (default: 10 MB)</param>
    public InMemoryFileStorageService(long maxFileSize = 10 * 1024 * 1024)
    {
        _maxFileSize = maxFileSize;
    }

    /// <inheritdoc />
    public async Task<FileUploadItem> SaveFileAsync(IBrowserFile file, CancellationToken cancellationToken = default)
    {
        var fileId = Guid.NewGuid().ToString();
        
        using var memoryStream = new MemoryStream();
        await file.OpenReadStream(_maxFileSize, cancellationToken).CopyToAsync(memoryStream, cancellationToken);
        var fileData = memoryStream.ToArray();

        _fileStorage[fileId] = fileData;

        return new FileUploadItem
        {
            FileName = file.Name,
            FileSize = fileData.Length,
            ContentType = file.ContentType,
            UploadDate = DateTime.Now,
            FileData = System.Text.Encoding.UTF8.GetBytes(fileId) // Store the ID for retrieval
        };
    }

    /// <inheritdoc />
    public Task<bool> DeleteFileAsync(FileUploadItem fileItem, CancellationToken cancellationToken = default)
    {
        var fileId = GetFileId(fileItem);
        return Task.FromResult(_fileStorage.TryRemove(fileId, out _));
    }

    /// <inheritdoc />
    public Task<Stream> GetFileStreamAsync(FileUploadItem fileItem, CancellationToken cancellationToken = default)
    {
        var fileId = GetFileId(fileItem);
        
        if (!_fileStorage.TryGetValue(fileId, out var fileData))
        {
            throw new FileNotFoundException($"File not found: {fileItem.FileName}");
        }

        return Task.FromResult<Stream>(new MemoryStream(fileData));
    }

    /// <inheritdoc />
    public Task<byte[]> GetFileBytesAsync(FileUploadItem fileItem, CancellationToken cancellationToken = default)
    {
        var fileId = GetFileId(fileItem);
        
        if (!_fileStorage.TryGetValue(fileId, out var fileData))
        {
            throw new FileNotFoundException($"File not found: {fileItem.FileName}");
        }

        return Task.FromResult(fileData);
    }

    /// <inheritdoc />
    public Task<bool> FileExistsAsync(FileUploadItem fileItem, CancellationToken cancellationToken = default)
    {
        var fileId = GetFileId(fileItem);
        return Task.FromResult(_fileStorage.ContainsKey(fileId));
    }

    private static string GetFileId(FileUploadItem fileItem)
    {
        if (fileItem.FileData == null || fileItem.FileData.Length == 0)
        {
            throw new InvalidOperationException("FileData is empty. Cannot determine file ID.");
        }

        return System.Text.Encoding.UTF8.GetString(fileItem.FileData);
    }

    /// <summary>
    /// Gets the current number of files in storage
    /// </summary>
    public int FileCount => _fileStorage.Count;

    /// <summary>
    /// Clears all files from storage
    /// </summary>
    public void Clear() => _fileStorage.Clear();
}
