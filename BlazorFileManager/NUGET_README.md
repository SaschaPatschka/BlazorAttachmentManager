# ?? Blazor File Manager

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](https://opensource.org/licenses/MIT)

A comprehensive, production-ready Blazor component for file upload, download, and preview with advanced features.

## Key Features

- **Multiple Upload Methods**: Browse, Drag & Drop, Clipboard Paste (Ctrl+V)
- **Pluggable Storage**: Local filesystem, in-memory, or custom backends (Azure, AWS ready)
- **Upload Modes**: Automatic or manual (staged) upload workflow
- **Multi-Language**: Built-in English, German, French + custom translations (40+ labels)
- **Fully Customizable**: RenderFragment templates for all UI elements
- **Smart Previews**: Image thumbnails with automatic storage caching
- **Auto-Compression**: Reduce large images automatically with quality control
- **Validation**: File size, type, and count limits with user-friendly messages
- **Type-Safe**: Full C# 12 and .NET 8 support with nullable reference types
- **Responsive**: Mobile-friendly UI with touch support

## Quick Start

### 1. Installation

```bash
dotnet add package BlazorFileManager
```

### 2. Register Services (Program.cs)

```csharp
// Local filesystem storage
builder.Services.AddFileManagerLocalStorage("uploads");

// OR in-memory storage
builder.Services.AddFileManagerInMemoryStorage();

// OR custom storage (Azure, AWS, etc.)
builder.Services.AddFileManagerCustomStorage<YourStorageService>();
```

### 3. Use Component

```razor
@page "/upload"
@using BlazorFileManager.Components

<FileUploadManager 
    Title="Upload Files"
    @bind-Files="uploadedFiles"
    Options="options"
    StorageService="storageService" />

@code {
    [Inject] IFileStorageService? storageService { get; set; }

    private List<FileUploadItem> uploadedFiles = new();

    private FileUploadOptions options = new()
    {
        MaxFileSize = 10 * 1024 * 1024, // 10 MB
        MaxFileCount = 5,
        AllowedFileTypes = new() { "image/jpeg", "image/png", "application/pdf" },
        AllowPasteFromClipboard = true,
        AutoCompressImages = true
    };
}
```

## Advanced Usage

### Manual Upload (Staged Files)

```csharp
var options = new FileUploadOptions
{
    AutoUpload = false // Files staged in queue before manual upload
};
```

Users can review files before clicking "Upload" button.

### Multi-Language Support

```razor
<!-- German -->
<FileUploadManager Labels="FileUploadLabels.German" />

<!-- French -->
<FileUploadManager Labels="FileUploadLabels.French" />

<!-- Custom -->
<FileUploadManager Labels="@customLabels" />
```

Create custom translations:

```csharp
var customLabels = new FileUploadLabels
{
    DropzoneText = "Deine Übersetzung hier",
    UploadButton = "Hochladen ({0})",
    // ... 40+ customizable strings
};
```

### Custom Storage Backend

Implement `IFileStorageService` for Azure Blob, AWS S3, etc:

```csharp
public class AzureBlobStorageService : IFileStorageService
{
    public async Task<FileUploadItem> SaveFileAsync(IBrowserFile file)
    {
        // Upload to Azure Blob Storage
    }

    public async Task<byte[]?> GetFileBytesAsync(FileUploadItem file)
    {
        // Download from Azure
    }

    // ... implement other methods
}

// Register in Program.cs
builder.Services.AddFileManagerCustomStorage<AzureBlobStorageService>();
```

### Custom Templates

```razor
<FileUploadManager>
    <HeaderTemplate>
        <div class="custom-header">
            <h2>My Custom Header</h2>
        </div>
    </HeaderTemplate>

    <FileItemTemplate Context="file">
        <div class="custom-file-item">
            <img src="@file.ThumbnailUrl" />
            <span>@file.FileName</span>
        </div>
    </FileItemTemplate>
</FileUploadManager>
```

Available templates:
- `HeaderTemplate`
- `DropZoneTemplate`
- `FileItemTemplate`
- `UploadButtonTemplate`
- `PasteFromClipboardButtonTemplate`
- `FooterTemplate`

### Events

```razor
<FileUploadManager 
    OnFileUploaded="HandleFileUploaded"
    OnFileDeleted="HandleFileDeleted" />

@code {
    private async Task HandleFileUploaded(FileUploadItem file)
    {
        Console.WriteLine($"Uploaded: {file.FileName}");
        // Custom logic (database save, notification, etc.)
    }

    private async Task HandleFileDeleted(FileUploadItem file)
    {
        Console.WriteLine($"Deleted: {file.FileName}");
    }
}
```

## Features in Detail

### Clipboard Paste
Users can paste images directly from clipboard (Ctrl+V):
- Screenshots
- Copied images from browser
- Image data from any source
- Automatically generates timestamped filename

### Auto Image Compression
Large images are automatically compressed:
```csharp
var options = new FileUploadOptions
{
    AutoCompressImages = true,
    MaxImageDimension = 1920, // Max width/height
    CompressionQualityLevels = new[] { 0.9, 0.8, 0.7 } // Try multiple quality levels
};
```

### Validation
Built-in validation with user-friendly error messages:
- File size limits
- File type restrictions (MIME types)
- Maximum file count
- All messages customizable via `FileUploadLabels`


## Requirements

- .NET 8.0 or later
- Blazor Server or WebAssembly
- Modern browser with File API support

## What's Included

- Razor Components (FileUploadManager, FileUploadItem, etc.)
- Storage Service Interface & Implementations
- Dependency Injection Extensions
- JavaScript Interop (clipboard, compression)
- CSS Isolation Styles
- Multi-language Support (40+ labels)
- Complete XML Documentation

## Version 1.0 Highlights

- **Unified Upload Pipeline**: All file sources (browse, drag-drop, clipboard) use identical processing
- **Storage Thumbnails**: Image previews cached from storage service
- **Enhanced Multi-Language**: 40+ translatable strings with format placeholders
- **Manual Upload**: Deferred upload workflow with staged files queue
- **ClipboardBrowserFile**: Clipboard images as first-class `IBrowserFile` instances



## License

MIT License - see [LICENSE](https://github.com/SaschaPatschka/BlazorAttachmentManager/blob/master/LICENSE)

## ?? Contributing

Contributions, issues and feature requests are welcome!

- **Repository**: [GitHub](https://github.com/SaschaPatschka/BlazorAttachmentManager)
- **Issues**: [GitHub Issues](https://github.com/SaschaPatschka/BlazorAttachmentManager/issues)
- **Discussions**: [GitHub Discussions](https://github.com/SaschaPatschka/BlazorAttachmentManager/discussions)

## Show Your Support

If this project helped you, please give it a thumb up on [GitHub](https://github.com/SaschaPatschka/BlazorAttachmentManager)!

## Author

**Sascha Patschka**

- GitHub: [@SaschaPatschka](https://github.com/SaschaPatschka)

---

