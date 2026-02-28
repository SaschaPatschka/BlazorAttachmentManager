# BlazorFileManager - Schnellstart

## 🚀 Schnellstart

Dieses Blazor-Komponent ermöglicht einfaches Datei-Management mit Upload, Download und Vorschau.

### Installation in Ihrem Projekt

1. **Projektreferenz hinzufügen**

Fügen Sie in Ihrer `.csproj`-Datei folgende Referenz hinzu:

```xml
<ItemGroup>
  <ProjectReference Include="..\BlazorFileManager\BlazorFileManager.csproj" />
</ItemGroup>
```

2. **JavaScript einbinden**

In `wwwroot/index.html` (Blazor WebAssembly) oder `Pages/_Host.cshtml` / `Pages/_Layout.cshtml` (Blazor Server):

```html
<script src="_content/BlazorFileManager/fileUploadManager.js"></script>
```

3. **Namespace importieren**

In `_Imports.razor`:

```razor
@using BlazorFileManager.Components
```

### Einfachstes Beispiel

```razor
@page "/upload"
@using BlazorFileManager.Components

<h3>Datei-Upload</h3>

<FileUploadManager @bind-Files="uploadedFiles" />

@code {
    private List<FileUploadItem> uploadedFiles = new();
}
```

Das war's! 🎉

### Erweiterte Konfiguration

```razor
<FileUploadManager 
    @bind-Files="uploadedFiles"
    Title="Meine Dateien"
    Options="options" />

@code {
    private List<FileUploadItem> uploadedFiles = new();
    
    private FileUploadOptions options = new()
    {
        MaxFileCount = 5,
        MaxFileSize = 10 * 1024 * 1024, // 10MB
        AllowedFileTypes = new List<string> { "image/jpeg", "image/png", "application/pdf" },
        AllowDelete = true,
        AllowDragDrop = true,
        AllowPasteFromClipboard = true
    };
}
```

### Features aktivieren

#### ✅ Nur Bilder erlauben

```csharp
Options = new FileUploadOptions
{
    AllowedFileTypes = new List<string> { "image/jpeg", "image/png", "image/gif" },
    AcceptAttribute = "image/*"
}
```

#### ✅ Events behandeln

```razor
<FileUploadManager 
    @bind-Files="files"
    OnFileUploaded="OnUploaded"
    OnFileDeleted="OnDeleted" />

@code {
    private async Task OnUploaded(FileUploadItem file)
    {
        Console.WriteLine($"Hochgeladen: {file.FileName}");
        // Datei an Server senden, in Datenbank speichern, etc.
    }

    private async Task OnDeleted(FileUploadItem file)
    {
        Console.WriteLine($"Gelöscht: {file.FileName}");
    }
}
```

#### ✅ Custom Design mit Templates

```razor
<FileUploadManager @bind-Files="files">
    <HeaderTemplate>
        <h2 style="color: blue;">Mein Custom Header</h2>
    </HeaderTemplate>
    
    <FileItemTemplate Context="file">
        <div style="padding: 1rem; background: lightblue;">
            📎 @file.FileName - @file.FormattedFileSize
        </div>
    </FileItemTemplate>
</FileUploadManager>
```

#### ✅ Server-Upload implementieren

```razor
<FileUploadManager 
    @bind-Files="files"
    CustomFileProcessor="UploadToServer" />

@code {
    [Inject]
    private HttpClient Http { get; set; }

    private async Task<FileUploadItem> UploadToServer(IBrowserFile browserFile)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(browserFile.OpenReadStream(10 * 1024 * 1024));
        content.Add(fileContent, "file", browserFile.Name);

        var response = await Http.PostAsync("api/upload", content);
        var serverPath = await response.Content.ReadAsStringAsync();

        return new FileUploadItem
        {
            FileName = browserFile.Name,
            ContentType = browserFile.ContentType,
            FileSize = browserFile.Size,
            FilePath = serverPath
        };
    }
}
```

### Demo starten

Das Projekt enthält eine Demo-Seite mit vielen Beispielen:

```bash
# Demo-Komponente ist in FileManagerDemo.razor
# Verwenden Sie diese als Vorlage für Ihre Implementierung
```

### Tastenkombinationen

- **Strg+V** - Bild aus Zwischenablage einfügen (wenn aktiviert)
- **Drag & Drop** - Dateien in die Drop-Zone ziehen

### Konfigurationsoptionen im Überblick

| Option | Beschreibung | Standard |
|--------|--------------|----------|
| `MaxFileCount` | Max. Anzahl Dateien (0 = unbegrenzt) | 10 |
| `MaxFileSize` | Max. Dateigröße in Bytes | 10 MB |
| `AllowedFileTypes` | Erlaubte MIME-Types | Alle |
| `UploadPath` | Upload-Pfad | "uploads" |
| `AllowDelete` | Löschen erlauben | true |
| `AllowDragDrop` | Drag & Drop aktivieren | true |
| `AllowPasteFromClipboard` | Einfügen aus Zwischenablage | true |
| `ShowPreview` | Vorschau anzeigen | true |
| `AllowMultiple` | Mehrfachauswahl | true |

### Hilfe & Dokumentation

- **README.md** - Vollständige Dokumentation
- **EXAMPLES.md** - Detaillierte Beispiele für verschiedene Szenarien
- **FileManagerDemo.razor** - Interaktive Demo mit verschiedenen Konfigurationen

### Typische Anwendungsfälle

1. **Anhänge in Formularen** - Dokumente zu Tickets, Rechnungen, etc.
2. **Bildergalerien** - Produktbilder, Profilbilder
3. **Dokumenten-Management** - PDFs, Word-Dokumente kategorisieren
4. **Chat-Anhänge** - Dateien in Chat-Nachrichten
5. **File-Manager** - Vollständiges Dateiverwaltungssystem

---

Viel Erfolg mit BlazorFileManager! 🎯
