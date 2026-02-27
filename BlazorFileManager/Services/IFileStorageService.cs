using BlazorFileManager.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorFileManager.Services;

/// <summary>
/// Interface for file storage operations.
/// Implement this interface to create custom storage providers (local filesystem, Azure Blob, AWS S3, etc.)
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to the storage system
    /// </summary>
    /// <param name="file">The browser file to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A FileUploadItem containing information about the saved file</returns>
    Task<FileUploadItem> SaveFileAsync(IBrowserFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from the storage system
    /// </summary>
    /// <param name="fileItem">The file item to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    Task<bool> DeleteFileAsync(FileUploadItem fileItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the file content as a stream
    /// </summary>
    /// <param name="fileItem">The file item to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A stream containing the file content</returns>
    Task<Stream> GetFileStreamAsync(FileUploadItem fileItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the file content as a byte array
    /// </summary>
    /// <param name="fileItem">The file item to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A byte array containing the file content</returns>
    Task<byte[]> GetFileBytesAsync(FileUploadItem fileItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists in storage
    /// </summary>
    /// <param name="fileItem">The file item to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the file exists, false otherwise</returns>
    Task<bool> FileExistsAsync(FileUploadItem fileItem, CancellationToken cancellationToken = default);
}
