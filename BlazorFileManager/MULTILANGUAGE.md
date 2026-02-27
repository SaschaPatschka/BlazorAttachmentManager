# Multi-Language Support

Die `FileUploadManager` Komponente bietet vollständige Mehrsprachigkeitsunterstützung durch austauschbare Labels.

## Übersicht

Alle Texte, Buttons und Fehlermeldungen können über die `Labels` Property angepasst werden. Dies ermöglicht:
- Lokalisierung in beliebige Sprachen
- Anpassung aller Texte für spezifische Anforderungen
- Integration in bestehende Lokalisierungssysteme

## Vordefinierte Sprachen

### 1. **English (Standard)**

```razor
<FileUploadManager 
    Labels="@FileUploadLabels.English"
    @bind-Files="files" />

<!-- oder einfach weglassen -->
<FileUploadManager @bind-Files="files" />
```

### 2. **Deutsch**

```razor
<FileUploadManager 
    Labels="@FileUploadLabels.German"
    @bind-Files="files" />
```

**Beispiel-Texte:**
- Dropzone: "Dateien hier ablegen oder klicken zum Durchsuchen"
- Download: "⬇️ Herunterladen"
- Delete: "🗑️ Löschen"
- Error: "Maximale Anzahl von Dateien ({0}) erreicht."

### 3. **Français**

```razor
<FileUploadManager 
    Labels="@FileUploadLabels.French"
    @bind-Files="files" />
```

**Beispiel-Texte:**
- Dropzone: "Déposez les fichiers ici ou cliquez pour parcourir"
- Download: "⬇️ Télécharger"
- Delete: "🗑️ Supprimer"
- Error: "Nombre maximum de fichiers ({0}) atteint."

## Eigene Sprache erstellen

### Beispiel: Spanisch

```razor
@code {
    private FileUploadLabels spanishLabels = new()
    {
        // Dropzone
        DropzoneText = "Arrastra y suelta archivos aquí o haz clic para explorar",
        ClipboardHint = "También puedes pegar imágenes desde el portapapeles (Ctrl+V)",
        ClipboardButtonText = "📋 Pegar desde el portapapeles",
        
        // Pending Files
        PendingFilesTitle = "📋 Archivos pendientes",
        ClearAllButton = "Borrar todo",
        UploadButton = "⬆️ Subir {0} archivo(s)",
        UploadingText = "⏳ Subiendo...",
        RemoveFileButton = "✕",
        
        // File Actions
        DownloadButton = "⬇️ Descargar",
        DeleteButton = "🗑️ Eliminar",
        
        // Header
        FilesCountText = "{0} / {1} archivos",
        
        // Error Messages
        ErrorMaxFilesReached = "Número máximo de archivos ({0}) alcanzado.",
        ErrorFileTooLarge = "El archivo '{0}' excede el tamaño máximo de {1}.",
        ErrorFileTypeNotAllowed = "El tipo de archivo '{0}' no está permitido para '{1}'.",
        ErrorNoFilesToUpload = "No hay archivos para subir.",
        ErrorUploadInProgress = "La carga ya está en progreso.",
        ErrorInitialization = "Error de inicialización: {0}",
        ErrorJavaScriptModule = "El módulo JavaScript se está cargando... Por favor, inténtelo de nuevo en un momento.",
        ErrorComponentNotInitialized = "Componente no inicializado. Por favor, recargue la página.",
        ErrorJavaScript = "Error de JavaScript: {0}",
        ErrorReadingClipboard = "Error al leer el portapapeles: {0}",
        ErrorNoImageData = "No se recibieron datos de imagen.",
        ErrorDecodingImage = "Error al decodificar la imagen: {0}",
        ErrorMaxFilesReachedClipboard = "Número máximo de archivos ({0}) alcanzado.",
        ErrorImageTooLarge = "La imagen excede el tamaño máximo de {0}.",
        ErrorProcessingClipboardImage = "Error al procesar la imagen del portapapeles: {0}",
        ErrorDeletingFile = "Error al eliminar el archivo '{0}' del almacenamiento.",
        ErrorDeletingFileException = "Error al eliminar el archivo '{0}': {1}",
        ErrorDownloadingFile = "Error al descargar el archivo '{0}': {1}",
        
        // Compression Messages
        ImageCompressed = "✓ La imagen '{0}' fue comprimida: {1} → {2}",
        ImageCompressionFailed = "La imagen '{0}' no pudo ser comprimida suficientemente. {1}",
        ErrorDuringCompression = "Error al comprimir '{0}': {1}"
    };
}

<FileUploadManager Labels="@spanishLabels" @bind-Files="files" />
```

## Dynamische Sprachwahl

```razor
@page "/filemanager"

<div>
    <label>Sprache / Language:</label>
    <select @onchange="OnLanguageChanged">
        <option value="en">English</option>
        <option value="de">Deutsch</option>
        <option value="fr">Français</option>
        <option value="es">Español</option>
    </select>
</div>

<FileUploadManager 
    Labels="@currentLabels"
    @bind-Files="files" />

@code {
    private string currentLanguage = "en";
    private FileUploadLabels currentLabels = FileUploadLabels.English;
    private List<FileUploadItem> files = new();

    private void OnLanguageChanged(ChangeEventArgs e)
    {
        currentLanguage = e.Value?.ToString() ?? "en";
        
        currentLabels = currentLanguage switch
        {
            "de" => FileUploadLabels.German,
            "fr" => FileUploadLabels.French,
            "es" => CreateSpanishLabels(),
            _ => FileUploadLabels.English
        };
    }

    private FileUploadLabels CreateSpanishLabels()
    {
        return new FileUploadLabels
        {
            DropzoneText = "Arrastra y suelta archivos aquí",
            DownloadButton = "⬇️ Descargar",
            DeleteButton = "🗑️ Eliminar",
            // ... weitere Labels
        };
    }
}
```

## Integration mit IStringLocalizer

Falls Sie bereits `IStringLocalizer` verwenden:

```csharp
@inject IStringLocalizer<FileManagerResources> Localizer

@code {
    private FileUploadLabels GetLocalizedLabels()
    {
        return new FileUploadLabels
        {
            DropzoneText = Localizer["DropzoneText"],
            ClipboardHint = Localizer["ClipboardHint"],
            ClipboardButtonText = Localizer["ClipboardButtonText"],
            DownloadButton = Localizer["DownloadButton"],
            DeleteButton = Localizer["DeleteButton"],
            ErrorMaxFilesReached = Localizer["ErrorMaxFilesReached"],
            // ... weitere Labels
        };
    }
}

<FileUploadManager Labels="@GetLocalizedLabels()" />
```

## Verfügbare Labels

### **UI-Elemente**

| Property | Verwendung | Default (English) |
|----------|------------|-------------------|
| `DropzoneText` | Haupttext in der Dropzone | "Drag & Drop files here or click to browse" |
| `ClipboardHint` | Hinweis für Zwischenablage | "You can also paste images from clipboard (Ctrl+V)" |
| `ClipboardButtonText` | Zwischenablage-Button | "📋 Paste from Clipboard" |
| `PendingFilesTitle` | Titel der Pending Files Liste | "📋 Pending Files" |
| `ClearAllButton` | Button zum Löschen aller Pending Files | "Clear All" |
| `UploadButton` | Upload-Button Text (mit {0} für Anzahl) | "⬆️ Upload {0} File(s)" |
| `UploadingText` | Text während Upload | "⏳ Uploading..." |
| `RemoveFileButton` | Button zum Entfernen einzelner Files | "✕" |
| `DownloadButton` | Download-Button | "⬇️ Download" |
| `DeleteButton` | Delete-Button | "🗑️ Delete" |
| `FilesCountText` | Dateizähler (mit {0} und {1}) | "{0} / {1} files" |

### **Fehlermeldungen**

Alle Error Messages unterstützen Platzhalter wie `{0}`, `{1}` etc.:

| Property | Platzhalter |
|----------|-------------|
| `ErrorMaxFilesReached` | {0} = max count |
| `ErrorFileTooLarge` | {0} = filename, {1} = max size |
| `ErrorFileTypeNotAllowed` | {0} = content type, {1} = filename |
| `ErrorDeletingFileException` | {0} = filename, {1} = error message |
| `ErrorDownloadingFile` | {0} = filename, {1} = error message |
| `ImageCompressed` | {0} = filename, {1} = original size, {2} = compressed size |

## Best Practices

### 1. **Zentrale Labels-Verwaltung**

Erstellen Sie eine zentrale Klasse für Ihre Labels:

```csharp
public static class AppLabels
{
    public static FileUploadLabels GetLabels(string culture)
    {
        return culture switch
        {
            "de-DE" => FileUploadLabels.German,
            "fr-FR" => FileUploadLabels.French,
            "es-ES" => CreateSpanishLabels(),
            _ => FileUploadLabels.English
        };
    }
}
```

### 2. **Verwendung mit CascadingParameter**

```razor
<!-- _Layout.razor -->
<CascadingValue Value="@currentLabels">
    @Body
</CascadingValue>

@code {
    private FileUploadLabels currentLabels = FileUploadLabels.English;
}

<!-- FileManager.razor -->
@code {
    [CascadingParameter]
    public FileUploadLabels Labels { get; set; } = FileUploadLabels.English;
}

<FileUploadManager Labels="@Labels" />
```

### 3. **State Management**

Speichern Sie die Sprachwahl im Local Storage:

```csharp
@inject IJSRuntime JS

private async Task SaveLanguagePreference(string language)
{
    await JS.InvokeVoidAsync("localStorage.setItem", "preferredLanguage", language);
}

private async Task<string> LoadLanguagePreference()
{
    return await JS.InvokeAsync<string>("localStorage.getItem", "preferredLanguage") ?? "en";
}
```

## Beispiel: Vollständige Integration

```razor
@page "/filemanager"
@inject IStringLocalizer<Resources> Localizer
@inject IJSRuntime JS

<div class="language-selector">
    <select @bind="selectedLanguage" @bind:after="OnLanguageChanged">
        <option value="en">English</option>
        <option value="de">Deutsch</option>
        <option value="fr">Français</option>
    </select>
</div>

<FileUploadManager 
    Labels="@currentLabels"
    Title="@Localizer["FileManagerTitle"]"
    @bind-Files="files" />

@code {
    private string selectedLanguage = "en";
    private FileUploadLabels currentLabels = FileUploadLabels.English;
    private List<FileUploadItem> files = new();

    protected override async Task OnInitializedAsync()
    {
        // Lade gespeicherte Sprachwahl
        selectedLanguage = await LoadLanguagePreference();
        UpdateLabels();
    }

    private async Task OnLanguageChanged()
    {
        UpdateLabels();
        await SaveLanguagePreference(selectedLanguage);
    }

    private void UpdateLabels()
    {
        currentLabels = selectedLanguage switch
        {
            "de" => FileUploadLabels.German,
            "fr" => FileUploadLabels.French,
            _ => FileUploadLabels.English
        };
    }

    private async Task SaveLanguagePreference(string language)
    {
        await JS.InvokeVoidAsync("localStorage.setItem", "preferredLanguage", language);
    }

    private async Task<string> LoadLanguagePreference()
    {
        return await JS.InvokeAsync<string>("localStorage.getItem", "preferredLanguage") ?? "en";
    }
}
```

## Erweiterung für weitere Sprachen

Sie können beliebige weitere Sprachen hinzufügen. Hier einige Beispiele:

### Italienisch
```csharp
public static FileUploadLabels Italian => new()
{
    DropzoneText = "Trascina e rilascia i file qui o fai clic per sfogliare",
    DownloadButton = "⬇️ Scarica",
    DeleteButton = "🗑️ Elimina",
    // ...
};
```

### Niederländisch
```csharp
public static FileUploadLabels Dutch => new()
{
    DropzoneText = "Sleep bestanden hierheen of klik om te bladeren",
    DownloadButton = "⬇️ Downloaden",
    DeleteButton = "🗑️ Verwijderen",
    // ...
};
```

### Polnisch
```csharp
public static FileUploadLabels Polish => new()
{
    DropzoneText = "Przeciągnij i upuść pliki tutaj lub kliknij, aby przeglądać",
    DownloadButton = "⬇️ Pobierz",
    DeleteButton = "🗑️ Usuń",
    // ...
};
```

## Zusammenfassung

✅ **3 vordefinierte Sprachen** (English, Deutsch, Français)  
✅ **Alle Texte überschreibbar** (40+ Labels)  
✅ **Einfache Integration** mit IStringLocalizer  
✅ **Platzhalter-Unterstützung** für dynamische Texte  
✅ **Vollständig typsicher** durch C#-Klasse  
