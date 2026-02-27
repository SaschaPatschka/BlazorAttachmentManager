using Microsoft.AspNetCore.Components.Forms;

namespace BlazorFileManager.Components;

/// <summary>
/// Implementation of IBrowserFile for clipboard images.
/// Allows clipboard data to be processed through the same pipeline as regular file uploads.
/// </summary>
internal class ClipboardBrowserFile : IBrowserFile
{
    private readonly byte[] _data;
    private readonly string _contentType;
    private readonly string _name;

    public ClipboardBrowserFile(byte[] data, string contentType)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _contentType = contentType ?? "image/png";
        _name = $"clipboard-image-{DateTime.Now:yyyyMMdd-HHmmss}.png";
    }

    public string Name => _name;

    public DateTimeOffset LastModified => DateTimeOffset.Now;

    public long Size => _data.Length;

    public string ContentType => _contentType;

    public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
    {
        if (_data.Length > maxAllowedSize)
        {
            throw new IOException($"The file size ({_data.Length} bytes) exceeds the maximum allowed size ({maxAllowedSize} bytes).");
        }

        return new MemoryStream(_data);
    }
}
