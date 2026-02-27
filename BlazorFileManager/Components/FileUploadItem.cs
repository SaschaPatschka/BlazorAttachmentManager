namespace BlazorFileManager.Components;

/// <summary>
/// Represents a file item in the upload manager
/// </summary>
public class FileUploadItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; } = DateTime.Now;
    public byte[]? FileData { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    public bool IsPdf => ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);
    
    public string FormattedFileSize
    {
        get
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = FileSize;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
