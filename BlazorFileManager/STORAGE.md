# File Storage Services

Die `FileUploadManager` Komponente unterstützt verschiedene Storage-Backends über das `IFileStorageService` Interface.

## Übersicht

Standardmäßig speichert die Komponente Dateien **in-memory** (im Arbeitsspeicher). Für produktive Anwendungen können Sie einen **Storage Service** verwenden, um Dateien persistent zu speichern.

## Verfügbare Storage Implementierungen

### 1. **InMemoryFileStorageService** (Standard für Tests/Demo)
- Speichert Dateien im RAM
- Dateien gehen beim Neustart verloren
- Ideal für Tests und Demos
- Keine Konfiguration erforderlich

### 2. **LocalFileStorageService** (Lokales Dateisystem)
- Speichert Dateien auf dem Server-Dateisystem
- Dateien bleiben nach Neustart erhalten
- Konfigurierbar (Upload-Ordner, max. Dateigröße)
- Ideal für kleinere Anwendungen

### 3. **Custom Implementation** (Eigene Implementierung)
- Azure Blob Storage
- AWS S3
- Database Binary Storage
- FTP Server
- Oder jeder andere Storage-Provider

## Installation und Konfiguration

### Option 1: In-Memory Storage (für Tests)

**Program.cs:**
```csharp
using BlazorFileManager.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Registriere In-Memory Storage
builder.Services.AddFileManagerInMemoryStorage();

var app = builder.Build();
app.Run();
```

**Verwendung in Razor-Komponente:**
```razor
@inject IFileStorageService StorageService

<FileUploadManager StorageService="@StorageService" />
```

---

### Option 2: Local File System Storage

**Program.cs:**
```csharp
using BlazorFileManager.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Option A: Standard-Pfad (./uploads)
builder.Services.AddFileManagerLocalStorage();

// Option B: Eigener Pfad
builder.Services.AddFileManagerLocalStorage(
    basePath: "C:\\MyApp\\Uploads",
    maxFileSize: 50 * 1024 * 1024  // 50 MB
);

var app = builder.Build();
app.Run();
```

**Verwendung in Razor-Komponente:**
```razor
@inject IFileStorageService StorageService

<FileUploadManager 
    StorageService="@StorageService"
    Options="@(new FileUploadOptions 
    { 
        AllowMultiple = true,
        MaxFileCount = 10
    })" />
```

---

### Option 3: Per-Component Storage (ohne DI)

Sie können den Storage Service auch direkt an die Komponente übergeben:

```razor
@code {
    private IFileStorageService storageService = new LocalFileStorageService("./uploads");
}

<FileUploadManager StorageService="@storageService" />
```

---

## Eigene Storage-Implementierung erstellen

### Schritt 1: Interface implementieren

```csharp
using BlazorFileManager.Services;
using BlazorFileManager.Components;
using Microsoft.AspNetCore.Components.Forms;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(string connectionString, string containerName)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<FileUploadItem> SaveFileAsync(
        IBrowserFile file, 
        CancellationToken cancellationToken = default)
    {
        var blobName = $"{Guid.NewGuid()}_{file.Name}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
        await blobClient.UploadAsync(stream, cancellationToken);

        return new FileUploadItem
        {
            FileName = file.Name,
            FileSize = file.Size,
            ContentType = file.ContentType,
            UploadDate = DateTime.UtcNow,
            // Speichere Blob-Name für späteren Abruf
            FileData = System.Text.Encoding.UTF8.GetBytes(blobName)
        };
    }

    public async Task<bool> DeleteFileAsync(
        FileUploadItem fileItem, 
        CancellationToken cancellationToken = default)
    {
        var blobName = System.Text.Encoding.UTF8.GetString(fileItem.FileData!);
        var blobClient = _containerClient.GetBlobClient(blobName);
        return await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<Stream> GetFileStreamAsync(
        FileUploadItem fileItem, 
        CancellationToken cancellationToken = default)
    {
        var blobName = System.Text.Encoding.UTF8.GetString(fileItem.FileData!);
        var blobClient = _containerClient.GetBlobClient(blobName);
        var downloadResult = await blobClient.DownloadAsync(cancellationToken);
        return downloadResult.Value.Content;
    }

    public async Task<byte[]> GetFileBytesAsync(
        FileUploadItem fileItem, 
        CancellationToken cancellationToken = default)
    {
        using var stream = await GetFileStreamAsync(fileItem, cancellationToken);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    public async Task<bool> FileExistsAsync(
        FileUploadItem fileItem, 
        CancellationToken cancellationToken = default)
    {
        var blobName = System.Text.Encoding.UTF8.GetString(fileItem.FileData!);
        var blobClient = _containerClient.GetBlobClient(blobName);
        return await blobClient.ExistsAsync(cancellationToken);
    }
}
```

### Schritt 2: Service registrieren

**Program.cs:**
```csharp
using BlazorFileManager.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Eigene Implementierung registrieren
builder.Services.AddFileManagerCustomStorage<AzureBlobStorageService>();

// ODER: Manuell mit Konfiguration
builder.Services.AddScoped<IFileStorageService>(sp => 
    new AzureBlobStorageService(
        connectionString: builder.Configuration["Azure:BlobStorage:ConnectionString"],
        containerName: "uploads"
    ));

var app = builder.Build();
app.Run();
```

---

## Best Practices

### 1. **Validierung**
Nutzen Sie die `FileUploadOptions` für Dateivalidierung:

```razor
<FileUploadManager 
    StorageService="@StorageService"
    Options="@(new FileUploadOptions 
    {
        MaxFileSize = 10 * 1024 * 1024,  // 10 MB
        AllowedFileTypes = new List<string> { "image/jpeg", "image/png", "application/pdf" },
        MaxFileCount = 5
    })" />
```

### 2. **Fehlerbehandlung**
Die Storage Services werfen Exceptions bei Fehlern. Die Komponente fängt diese ab und zeigt Fehlermeldungen an.

### 3. **Sicherheit**
- **Dateinamen sanitizen**: Die `LocalFileStorageService` macht dies automatisch
- **Dateitypen validieren**: Nutzen Sie `AllowedFileTypes`
- **Dateigröße limitieren**: Nutzen Sie `MaxFileSize`
- **Zugriffskontrolle**: Implementieren Sie Autorisierung in Ihrer Storage-Klasse

### 4. **Performance**
- Für große Dateien: Stream-basiertes Verarbeiten
- Für viele kleine Dateien: Batch-Operationen implementieren
- Azure/AWS: Nutzen Sie CDN für Downloads

---

## Beispiel: Vollständige Integration

**Program.cs:**
```csharp
using BlazorFileManager.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Storage Service registrieren
builder.Services.AddFileManagerLocalStorage(
    basePath: Path.Combine(builder.Environment.ContentRootPath, "uploads"),
    maxFileSize: 50 * 1024 * 1024
);

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

**FileManagerPage.razor:**
```razor
@page "/filemanager"
@inject IFileStorageService StorageService

<h3>File Upload Manager</h3>

<FileUploadManager 
    StorageService="@StorageService"
    Files="@uploadedFiles"
    FilesChanged="@((files) => uploadedFiles = files)"
    OnFileUploaded="@HandleFileUploaded"
    OnFileDeleted="@HandleFileDeleted"
    Options="@fileOptions" />

@code {
    private List<FileUploadItem> uploadedFiles = new();
    
    private FileUploadOptions fileOptions = new()
    {
        AllowMultiple = true,
        MaxFileCount = 10,
        MaxFileSize = 10 * 1024 * 1024,
        AllowedFileTypes = new List<string> 
        { 
            "image/jpeg", 
            "image/png", 
            "application/pdf" 
        },
        ShowPreview = true,
        AllowDelete = true
    };

    private void HandleFileUploaded(FileUploadItem file)
    {
        Console.WriteLine($"File uploaded: {file.FileName}");
    }

    private void HandleFileDeleted(FileUploadItem file)
    {
        Console.WriteLine($"File deleted: {file.FileName}");
    }
}
```

---

## Migration von In-Memory zu Storage Service

Wenn Sie bereits die Komponente mit In-Memory Storage verwenden:

1. **Service registrieren** (siehe oben)
2. **StorageService injizieren**:
   ```razor
   @inject IFileStorageService StorageService
   ```
3. **An Komponente übergeben**:
   ```razor
   <FileUploadManager StorageService="@StorageService" />
   ```

Die Komponente verwendet automatisch den Storage Service, wenn er bereitgestellt wird!
