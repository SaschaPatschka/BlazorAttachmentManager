# Manual Upload Feature

## Übersicht

Die `FileUploadManager` Komponente unterstützt zwei Upload-Modi:

1. **Auto Upload (Standard)** - Dateien werden sofort beim Auswählen hochgeladen
2. **Manual Upload** - Dateien werden gesammelt und müssen manuell hochgeladen werden

## Verwendung

### 1. Aktivieren des Manual Upload Modus

Setzen Sie `AutoUpload = false` in den Optionen:

```razor
@code {
    private FileUploadOptions options = new()
    {
        AutoUpload = false,  // ✨ Aktiviert Manual Upload
        MaxFileCount = 10,
        MaxFileSize = 10 * 1024 * 1024
    };
}
```

### 2. Standard Manual Upload mit Default Button

Der Upload-Button wird automatisch angezeigt:

```razor
<FileUploadManager 
    @bind-Files="uploadedFiles"
    Options="@options" />
```

**So sieht es aus:**
- Benutzer wählt Dateien aus (Drag & Drop, Browse, Clipboard)
- Dateien werden in "Pending Files" Liste angezeigt
- Benutzer klickt auf "Upload X File(s)" Button
- Alle Dateien werden gleichzeitig hochgeladen

### 3. Programmgesteuerter Upload

Sie können den Upload auch aus Ihrem Code auslösen:

```razor
<FileUploadManager 
    @ref="fileManager"
    @bind-Files="uploadedFiles"
    Options="@options" />

<button @onclick="UploadNow">Jetzt hochladen!</button>

@code {
    private FileUploadManager? fileManager;
    private List<FileUploadItem> uploadedFiles = new();

    private async Task UploadNow()
    {
        if (fileManager != null)
        {
            await fileManager.UploadFilesAsync();
        }
    }
}
```

### 4. Custom Upload Button

Gestalten Sie Ihren eigenen Upload-Button:

```razor
<FileUploadManager @ref="fileManager" Options="@options">
    <UploadButtonTemplate>
        <div class="custom-upload-section">
            <button class="my-custom-btn" 
                    @onclick="async () => await fileManager!.UploadFilesAsync()">
                🚀 Alle Dateien hochladen
            </button>
        </div>
    </UploadButtonTemplate>
</FileUploadManager>
```

### 5. Pending Files verwalten

Die Komponente bietet zusätzliche Methoden:

```razor
<button @onclick="ClearPending">Alle Dateien entfernen</button>

@code {
    private FileUploadManager? fileManager;

    private void ClearPending()
    {
        fileManager?.ClearPendingFiles();
    }

    private void RemoveSpecificFile(IBrowserFile file)
    {
        fileManager?.RemovePendingFile(file);
    }
}
```

## Features der Pending Files Liste

### Was wird angezeigt?
- Dateiname
- Dateigröße
- Anzahl der Dateien
- "Remove" Button für jede Datei
- "Clear All" Button

### Validierung
Dateien werden **bereits beim Hinzufügen** validiert:
- ✅ Dateigröße
- ✅ Dateityp
- ✅ Max. Anzahl

Ungültige Dateien werden **nicht** zur Pending-Liste hinzugefügt.

## Beispiel: Formulare mit File Upload

```razor
@page "/upload-form"
@using BlazorFileManager.Components

<EditForm Model="@model" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <div class="form-group">
        <label>Name</label>
        <InputText @bind-Value="model.Name" class="form-control" />
    </div>

    <div class="form-group">
        <label>Dateien</label>
        <FileUploadManager 
            @ref="fileManager"
            @bind-Files="model.Attachments"
            Options="@uploadOptions" />
    </div>

    <button type="submit" class="btn btn-primary">
        Absenden
    </button>
</EditForm>

@code {
    private FileUploadManager? fileManager;
    private FormModel model = new();

    private FileUploadOptions uploadOptions = new()
    {
        AutoUpload = false,  // Upload erst beim Submit
        MaxFileCount = 5
    };

    private async Task HandleSubmit()
    {
        // Upload Dateien
        if (fileManager != null)
        {
            await fileManager.UploadFilesAsync();
        }

        // Sende Formular
        await SaveToDatabase();
    }

    public class FormModel
    {
        public string Name { get; set; } = "";
        public List<FileUploadItem> Attachments { get; set; } = new();
    }
}
```

## Beispiel: Batch Upload mit Fortschrittsanzeige

```razor
<FileUploadManager 
    @ref="fileManager"
    @bind-Files="uploadedFiles"
    OnFileUploaded="HandleFileUploaded"
    Options="@options">
    
    <UploadButtonTemplate>
        <button @onclick="StartBatchUpload" disabled="@isUploading">
            @if (isUploading)
            {
                <span>⏳ Uploading @uploadedCount / @totalCount...</span>
            }
            else
            {
                <span>⬆️ Upload All Files</span>
            }
        </button>
    </UploadButtonTemplate>
</FileUploadManager>

@code {
    private FileUploadManager? fileManager;
    private bool isUploading = false;
    private int uploadedCount = 0;
    private int totalCount = 0;

    private async Task StartBatchUpload()
    {
        if (fileManager != null)
        {
            isUploading = true;
            uploadedCount = 0;
            // totalCount würde man über PendingFiles.Count bekommen
            StateHasChanged();

            await fileManager.UploadFilesAsync();

            isUploading = false;
            StateHasChanged();
        }
    }

    private void HandleFileUploaded(FileUploadItem file)
    {
        uploadedCount++;
        StateHasChanged();
    }
}
```

## Best Practices

### 1. **Verwenden Sie Manual Upload bei:**
- Formularen mit mehreren Feldern
- Batch-Uploads mit Fortschrittsanzeige
- Wenn Benutzer Upload kontrollieren sollen
- Wenn Validierung vor Upload erforderlich ist

### 2. **Verwenden Sie Auto Upload bei:**
- Einfachen Upload-Szenarien
- Wenn sofortiges Feedback gewünscht ist
- Drag & Drop Fokus
- Wenn keine weiteren Formularfelder vorhanden sind

### 3. **UI/UX Tipps:**
- Zeigen Sie die Anzahl der Pending Files deutlich an
- Geben Sie Feedback während des Uploads
- Erlauben Sie das Entfernen einzelner Dateien
- Deaktivieren Sie den Upload-Button während des Uploads

## API Referenz

### FileUploadOptions
```csharp
public class FileUploadOptions
{
    public bool AutoUpload { get; set; } = true;
}
```

### FileUploadManager Methods
```csharp
// Upload alle Pending Files
public async Task UploadFilesAsync()

// Entferne eine Pending File
public void RemovePendingFile(IBrowserFile file)

// Lösche alle Pending Files
public void ClearPendingFiles()
```

### Templates
```razor
<UploadButtonTemplate>
    <!-- Ihr Custom Button -->
</UploadButtonTemplate>
```

## Troubleshooting

**Problem:** Upload-Button wird nicht angezeigt
- **Lösung:** Stellen Sie sicher, dass `AutoUpload = false` gesetzt ist

**Problem:** `UploadFilesAsync()` macht nichts
- **Lösung:** Überprüfen Sie, ob Dateien in der Pending-Liste sind

**Problem:** Dateien werden sofort hochgeladen
- **Lösung:** Überprüfen Sie die `AutoUpload` Option in `FileUploadOptions`
