# BlazorFileManager

Ein wiederverwendbares Blazor-Komponent für Datei-Upload, -Download und -Vorschau.

## Features

- ✅ **Konsistente Dateiverarbeitung** - Alle Quellen (Browse, Drag&Drop, Clipboard) gleich behandelt
- ✅ Drag & Drop Datei-Upload
- ✅ Bilder aus Zwischenablage einfügen (Strg+V)
- ✅ Datei-Vorschau (Bilder, PDFs)
- ✅ Datei-Download
- ✅ Datei-Löschen (konfigurierbar)
- ✅ **Storage Services** (Local, In-Memory, Custom) - für alle Dateiquellen
- ✅ **Manual Upload** - Dateien sammeln und auf Knopfdruck hochladen
- ✅ **Multi-Language Support** - English, Deutsch, Français + Custom
- ✅ **Auto Compression** - Automatische Bildkomprimierung
- ✅ Umfangreiche Konfigurationsoptionen
- ✅ Anpassbare Templates für flexibles Design
- ✅ Validierung (Dateigröße, Dateityp, Anzahl)
- ✅ Öffentliche API für programmgesteuerten Upload

## 🆕 Version 2.0 - Konsistente Dateiverarbeitung

**Alle Dateien werden jetzt gleich behandelt!**

- ✅ Clipboard-Bilder nutzen Storage Service (wenn konfiguriert)
- ✅ Clipboard-Bilder respektieren AutoUpload Setting
- ✅ Clipboard-Bilder erscheinen in Pending Files (bei AutoUpload=false)
- ✅ Eine Upload-Pipeline für alle Quellen
- ✅ Siehe [CHANGELOG.md](CHANGELOG.md) für Details

## Installation

1. Fügen Sie das Projekt als Referenz hinzu
2. Importieren Sie den Namespace in Ihrer `_Imports.razor`:

```razor
@using BlazorFileManager.Components
```

3. Fügen Sie das JavaScript in Ihrer `index.html` oder `_Layout.cshtml` hinzu:

```html
<script src="_content/BlazorFileManager/fileUploadManager.js"></script>
```

## Verwendung

### Einfaches Beispiel

```razor
<FileUploadManager @bind-Files="uploadedFiles" />

@code {
    private List<FileUploadItem> uploadedFiles = new();
}
```

### Mit Konfiguration

```razor
<FileUploadManager 
    @bind-Files="uploadedFiles"
    Title="Anhänge verwalten"
    Options="uploadOptions" 
    OnFileUploaded="HandleFileUploaded"
    OnFileDeleted="HandleFileDeleted" />

@code {
    private List<FileUploadItem> uploadedFiles = new();
    
    private FileUploadOptions uploadOptions = new()
    {
        MaxFileCount = 5,
        MaxFileSize = 5 * 1024 * 1024, // 5MB
        AllowedFileTypes = new List<string> { "image/jpeg", "image/png", "application/pdf" },
        UploadPath = "attachments",
        AllowDelete = true,
        AllowDragDrop = true,
        AllowPasteFromClipboard = true,
        ShowPreview = true
    };

    private Task HandleFileUploaded(FileUploadItem file)
    {
        Console.WriteLine($"Datei hochgeladen: {file.FileName}");
        return Task.CompletedTask;
    }

    private Task HandleFileDeleted(FileUploadItem file)
    {
        Console.WriteLine($"Datei gelöscht: {file.FileName}");
        return Task.CompletedTask;
    }
}
```

### Mit Manual Upload (verzögerter Upload)

```razor
<FileUploadManager 
    @ref="fileManager"
    @bind-Files="files"
    Options="manualOptions" />

<button @onclick="UploadNow">Jetzt hochladen!</button>

@code {
    private FileUploadManager? fileManager;
    private List<FileUploadItem> files = new();

    private FileUploadOptions manualOptions = new()
    {
        AutoUpload = false,  // ✨ Dateien werden nicht sofort hochgeladen
        MaxFileCount = 10
    };

    private async Task UploadNow()
    {
        if (fileManager != null)
        {
            await fileManager.UploadFilesAsync();
        }
    }
}
```

### Mit Custom Upload Button

```razor
<FileUploadManager @ref="fileManager" Options="manualOptions">
    <UploadButtonTemplate>
        <button class="custom-btn" @onclick="async () => await fileManager!.UploadFilesAsync()">
            🚀 Alle Dateien hochladen
        </button>
    </UploadButtonTemplate>
</FileUploadManager>
```

### Mit automatischer Bildkomprimierung

```razor
<FileUploadManager 
    @bind-Files="files"
    Options="options" />

@code {
    private List<FileUploadItem> files = new();

    private FileUploadOptions options = new()
    {
        MaxFileSize = 500 * 1024, // 500 KB
        AutoCompressImages = true, // 🔥 Aktiviert automatische Komprimierung
        CompressionQualityLevels = new List<double> { 0.9, 0.8, 0.7, 0.6, 0.5 },
        MaxImageDimension = 1920,
        AllowedFileTypes = new List<string> { "image/jpeg", "image/png" }
    };
}
```

**So funktioniert es:**
- Bilder, die größer als `MaxFileSize` sind, werden automatisch komprimiert
- Die Komprimierung erfolgt in mehreren Stufen (90%, 80%, 70%, 60%, 50%)
- Falls nötig, werden Bilder auf max. 1920px Breite/Höhe verkleinert
- Der Benutzer erhält Feedback über die Komprimierung

### Mit Multi-Language Support

```razor
<!-- Deutsch -->
<FileUploadManager 
    Labels="@FileUploadLabels.German"
    @bind-Files="files" />

<!-- Français -->
<FileUploadManager 
    Labels="@FileUploadLabels.French"
    @bind-Files="files" />

<!-- Custom (z.B. Spanisch) -->
<FileUploadManager 
    Labels="@customLabels"
    @bind-Files="files" />

@code {
    private FileUploadLabels customLabels = new()
    {
        DropzoneText = "Arrastra y suelta archivos aquí o haz clic para explorar",
        DownloadButton = "⬇️ Descargar",
        DeleteButton = "🗑️ Eliminar",
        // ... weitere Labels
    };
}
```

### Mit Custom File Processing (z.B. Server-Upload)

```razor
<FileUploadManager 
    @bind-Files="uploadedFiles"
    CustomFileProcessor="ProcessFileOnServer" />

@code {
    private List<FileUploadItem> uploadedFiles = new();

    [Inject]
    private HttpClient Http { get; set; }

    private async Task<FileUploadItem> ProcessFileOnServer(IBrowserFile browserFile)
    {
        // Upload to server
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(browserFile.OpenReadStream(10 * 1024 * 1024));
        content.Add(fileContent, "file", browserFile.Name);

        var response = await Http.PostAsync("api/upload", content);
        var uploadedFilePath = await response.Content.ReadAsStringAsync();

        return new FileUploadItem
        {
            FileName = browserFile.Name,
            ContentType = browserFile.ContentType,
            FileSize = browserFile.Size,
            FilePath = uploadedFilePath,
            UploadDate = DateTime.Now
        };
    }
}
```

### Mit benutzerdefinierten Templates

```razor
<FileUploadManager @bind-Files="uploadedFiles">
    <HeaderTemplate>
        <div style="background: linear-gradient(to right, #667eea 0%, #764ba2 100%); padding: 1rem; color: white; border-radius: 8px;">
            <h2>Meine Dateien</h2>
            <p>@uploadedFiles.Count Dateien hochgeladen</p>
        </div>
    </HeaderTemplate>

    <DropZoneTemplate>
        <div style="padding: 3rem; text-align: center;">
            <h3>📤 Dateien hier ablegen</h3>
            <p>oder klicken zum Durchsuchen</p>
        </div>
    </DropZoneTemplate>

    <FileItemTemplate Context="file">
        <div style="display: flex; gap: 1rem; padding: 1rem; border: 2px solid #667eea; border-radius: 8px; margin-bottom: 0.5rem;">
            <strong>@file.FileName</strong>
            <span>@file.FormattedFileSize</span>
        </div>
    </FileItemTemplate>

    <FooterTemplate>
        <div style="margin-top: 1rem; padding: 1rem; background: #f5f5f5; border-radius: 8px;">
            <small>Gesamt: @uploadedFiles.Sum(f => f.FileSize) Bytes</small>
        </div>
    </FooterTemplate>
</FileUploadManager>
```

## Konfigurationsoptionen (FileUploadOptions)

| Property | Typ | Standard | Beschreibung |
|----------|-----|----------|--------------|
| `MaxFileCount` | `int` | 10 | Maximale Anzahl der Dateien (0 = unbegrenzt) |
| `MaxFileSize` | `long` | 10 MB | Maximale Dateigröße in Bytes |
| `AllowedFileTypes` | `List<string>` | Leer (alle) | Erlaubte MIME-Typen |
| `UploadPath` | `string` | "uploads" | Upload-Pfad |
| `AllowDelete` | `bool` | true | Löschen erlauben |
| `AllowDragDrop` | `bool` | true | Drag & Drop erlauben |
| `AllowPasteFromClipboard` | `bool` | true | Einfügen aus Zwischenablage erlauben |
| `ShowPreview` | `bool` | true | Vorschau anzeigen |
| `AllowMultiple` | `bool` | true | Mehrfachauswahl erlauben |
| `AcceptAttribute` | `string?` | null | Accept-Attribut für Input (z.B. "image/*") |
| `AutoUpload` | `bool` | true | **Sofortiger Upload** oder verzögerter Upload |

## Public Methods

- `UploadFilesAsync()` - Lädt alle ausstehenden Dateien hoch (nur wenn `AutoUpload = false`)
- `RemovePendingFile(IBrowserFile)` - Entfernt eine ausstehende Datei aus der Warteschlange
- `ClearPendingFiles()` - Löscht alle ausstehenden Dateien

## Events

- `OnFileUploaded` - Wird ausgelöst, wenn eine Datei hochgeladen wurde
- `OnFileDeleted` - Wird ausgelöst, wenn eine Datei gelöscht wurde
- `OnFileDownloaded` - Wird ausgelöst, wenn eine Datei heruntergeladen wurde
- `FilesChanged` - Wird ausgelöst, wenn sich die Dateiliste ändert

## Templates

- `HeaderTemplate` - Benutzerdefinierter Header
- `DropZoneTemplate` - Benutzerdefinierte Drop-Zone
- `FileItemTemplate` - Benutzerdefinierte Datei-Anzeige
- `UploadButtonTemplate` - Benutzerdefinierter Upload-Button (nur bei `AutoUpload = false`)
- `PasteFromClipboardButtonTemplate` - Benutzerdefinierter Zwischenablage-Button
- `FooterTemplate` - Benutzerdefinierter Footer

## File Storage Services

Die Komponente unterstützt verschiedene Storage-Backends:

### 1. In-Memory Storage (Standard)
```csharp
// Program.cs
builder.Services.AddFileManagerInMemoryStorage();
```

### 2. Local File System Storage
```csharp
// Program.cs
builder.Services.AddFileManagerLocalStorage(
    basePath: "C:\\Uploads",
    maxFileSize: 50 * 1024 * 1024  // 50 MB
);
```

### 3. Custom Storage (Azure Blob, AWS S3, etc.)
```csharp
// Implementiere IFileStorageService
public class AzureBlobStorageService : IFileStorageService
{
    public async Task<FileUploadItem> SaveFileAsync(IBrowserFile file, CancellationToken ct)
    {
        // Azure Blob Upload Logik
    }
    // ... weitere Methoden
}

// Program.cs
builder.Services.AddFileManagerCustomStorage<AzureBlobStorageService>();
```

### Verwendung in Komponente
```razor
@inject IFileStorageService StorageService

<FileUploadManager StorageService="@StorageService" />
```

**📖 Weitere Informationen:** Siehe [STORAGE.md](STORAGE.md) für detaillierte Dokumentation.

## Lizenz

MIT
