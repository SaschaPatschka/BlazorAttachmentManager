namespace BlazorFileManager.Components;

/// <summary>
/// Configuration options for the FileUploadManager component
/// </summary>
public class FileUploadOptions
{
    /// <summary>
    /// Maximum number of files that can be uploaded. Default is 10. Set to 0 for unlimited.
    /// </summary>
    public int MaxFileCount { get; set; } = 10;

    /// <summary>
    /// Maximum file size in bytes. Default is 10MB.
    /// </summary>
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Allowed file types (MIME types). Empty list means all types are allowed.
    /// </summary>
    public List<string> AllowedFileTypes { get; set; } = new();

    /// <summary>
    /// Upload path where files will be stored
    /// </summary>
    public string UploadPath { get; set; } = "uploads";

    /// <summary>
    /// Allow file deletion
    /// </summary>
    public bool AllowDelete { get; set; } = true;

    /// <summary>
    /// Allow drag and drop
    /// </summary>
    public bool AllowDragDrop { get; set; } = true;

    /// <summary>
    /// Allow paste from clipboard
    /// </summary>
    public bool AllowPasteFromClipboard { get; set; } = true;

    /// <summary>
    /// Show file preview
    /// </summary>
    public bool ShowPreview { get; set; } = true;

    /// <summary>
    /// Allow multiple file selection
    /// </summary>
    public bool AllowMultiple { get; set; } = true;

    /// <summary>
    /// Accept attribute for file input (e.g., "image/*,.pdf")
    /// </summary>
    public string? AcceptAttribute { get; set; }

    /// <summary>
    /// Automatically compress images that exceed MaxFileSize
    /// </summary>
    public bool AutoCompressImages { get; set; } = false;

    /// <summary>
    /// Quality levels for image compression (0.0 - 1.0). Will try each level until file size is acceptable.
    /// Default: [0.9, 0.8, 0.7, 0.6, 0.5]
    /// </summary>
    public List<double> CompressionQualityLevels { get; set; } = new() { 0.9, 0.8, 0.7, 0.6, 0.5 };

    /// <summary>
    /// Maximum width/height for compressed images. Images larger than this will be resized.
    /// Default: 1920 pixels
    /// </summary>
    public int MaxImageDimension { get; set; } = 1920;

    /// <summary>
    /// Automatically upload files immediately when selected.
    /// If false, files will be staged and must be uploaded manually via UploadFilesAsync() method or upload button.
    /// Default: true
    /// </summary>
    public bool AutoUpload { get; set; } = true;
}
