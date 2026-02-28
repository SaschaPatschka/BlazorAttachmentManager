# Clipboard Images Behavior

## Übersicht

Bilder, die aus der Zwischenablage eingefügt werden (Ctrl+V), werden ab Version 2.0 **konsistent** über die gleiche Pipeline wie normale Datei-Uploads verarbeitet.

## Verhalten (Version 2.0+)

### ✅ Konsistente Verarbeitung

**Clipboard-Bilder werden jetzt genauso behandelt wie normale Uploads:**

1. **Konvertierung zu IBrowserFile**
   ```csharp
   var clipboardFile = new ClipboardBrowserFile(imageBytes, contentType);
   ```

2. **Durch normale Upload-Pipeline**
   ```csharp
   if (Options.AutoUpload)
   {
       await ProcessFiles(new[] { clipboardFile });
   }
   else
   {
       StageFiles(new[] { clipboardFile });
   }
   ```

3. **Gleiche Behandlung wie alle anderen Dateien**
   - ✅ Respektiert `AutoUpload` Setting
   - ✅ Verwendet Storage Service (wenn konfiguriert)
   - ✅ Gleiche Validierung
   - ✅ Gleiche Error Handling
   - ✅ Gleiche Events (`OnFileUploaded`)

### Vergleich: Alt vs. Neu

| Feature | v1.0 (Alt) | v2.0 (Neu) |
|---------|------------|------------|
| Storage Service | ❌ Ignoriert | ✅ Verwendet |
| AutoUpload Setting | ❌ Ignoriert | ✅ Respektiert |
| Pending Files | ❌ Nie | ✅ Bei AutoUpload=false |
| Konsistenz | ❌ Spezielle Logik | ✅ Gleiche Pipeline |
| Löschen | ⚠️ Spezialbehandlung | ✅ Standard |

## Szenarien

### 1. AutoUpload = true (Standard)

**Mit Storage Service:**
```csharp
<FileUploadManager 
    StorageService="@storageService"
    Options="@(new FileUploadOptions { AutoUpload = true })" />
```

**Ablauf:**
1. Benutzer drückt Ctrl+V
2. Bild wird zu `ClipboardBrowserFile` konvertiert
3. `ProcessFiles()` wird aufgerufen
4. Datei wird über `StorageService.SaveFileAsync()` gespeichert
5. Datei erscheint in `Files` Liste
6. ✅ Persistent gespeichert (Disk/Cloud)

**Ohne Storage Service:**
1. Benutzer drückt Ctrl+V
2. Bild wird zu `ClipboardBrowserFile` konvertiert
3. `ProcessFiles()` wird aufgerufen
4. Default Processing: Datei wird in Memory gespeichert
5. Datei erscheint in `Files` Liste
6. ⚠️ Nur im Memory (verloren nach Neustart)

### 2. AutoUpload = false (Manual Upload)

**Mit Storage Service:**
```csharp
<FileUploadManager 
    StorageService="@storageService"
    Options="@(new FileUploadOptions { AutoUpload = false })" />
```

**Ablauf:**
1. Benutzer drückt Ctrl+V
2. Bild wird zu `ClipboardBrowserFile` konvertiert
3. `StageFiles()` wird aufgerufen
4. Datei erscheint in `PendingFiles` Liste
5. Benutzer klickt "Upload X File(s)" Button
6. `UploadFilesAsync()` → `ProcessFiles()`
7. Datei wird über `StorageService.SaveFileAsync()` gespeichert
8. ✅ Persistent gespeichert

### 3. Custom File Processor

```csharp
<FileUploadManager 
    CustomFileProcessor="@MyCustomProcessor"
    Options="@(new FileUploadOptions { AutoUpload = true })" />

@code {
    private async Task<FileUploadItem> MyCustomProcessor(IBrowserFile file)
    {
        // Diese Methode wird AUCH für Clipboard-Bilder aufgerufen!
        if (file is ClipboardBrowserFile)
        {
            // Spezielle Behandlung für Clipboard-Bilder
        }

        // Upload zu Custom Backend
        return await UploadToMyBackend(file);
    }
}
```

## Unterschied zu normalen Uploads

### Einziger Unterschied: Dateiname

| Eigenschaft | Normale Dateien | Clipboard-Bilder |
|-------------|-----------------|------------------|
| Dateiname | Original vom Benutzer | `clipboard-image-20240115-143022.png` |
| Speicherort | Gleich (Storage/Memory) | Gleich (Storage/Memory) |
| Upload-Logik | Gleich | Gleich |
| Validierung | Gleich | Gleich |
| Events | Gleich | Gleich |

## Technische Details

### ClipboardBrowserFile Klasse

```csharp
internal class ClipboardBrowserFile : IBrowserFile
{
    private readonly byte[] _data;
    private readonly string _contentType;
    private readonly string _name;

    public ClipboardBrowserFile(byte[] data, string contentType)
    {
        _data = data;
        _contentType = contentType;
        _name = $"clipboard-image-{DateTime.Now:yyyyMMdd-HHmmss}.png";
    }

    public string Name => _name;
    public long Size => _data.Length;
    public string ContentType => _contentType;

    public Stream OpenReadStream(long maxAllowedSize, CancellationToken ct)
    {
        if (_data.Length > maxAllowedSize)
            throw new IOException("File too large");

        return new MemoryStream(_data);
    }
}
```

### Ablauf in HandleClipboardImage

```csharp
[JSInvokable]
public async Task HandleClipboardImage(string base64Data, string contentType)
{
    // 1. Base64 dekodieren
    var imageBytes = Convert.FromBase64String(base64Data);

    // 2. In IBrowserFile konvertieren
    var clipboardFile = new ClipboardBrowserFile(imageBytes, contentType);

    // 3. Normale Upload-Pipeline verwenden
    if (Options.AutoUpload)
    {
        await ProcessFiles(new[] { clipboardFile });
    }
    else
    {
        StageFiles(new[] { clipboardFile });
    }
}
```

## Löschen von Dateien

Das Löschen funktioniert jetzt **einheitlich** für alle Dateien:

```csharp
private async Task DeleteFile(FileUploadItem file)
{
    if (StorageService != null)
    {
        // Prüfe ob Datei im Storage liegt
        bool existsInStorage = await StorageService.FileExistsAsync(file);

        if (existsInStorage)
        {
            // Aus Storage löschen
            await StorageService.DeleteFileAsync(file);
        }
    }

    // Aus Memory/Liste entfernen
    Files.Remove(file);
}
```

**Funktioniert für:**
- ✅ Normale Uploads im Storage
- ✅ Clipboard-Bilder im Storage
- ✅ Memory-only Dateien
- ✅ Pending Files (noch nicht hochgeladen)

## Best Practices

### ✅ Empfohlen

1. **Production: Storage Service verwenden**
   ```csharp
   builder.Services.AddFileManagerLocalStorage();
   // oder
   builder.Services.AddFileManagerCustomStorage<AzureBlobStorage>();
   ```

2. **AutoUpload für sofortige Persistenz**
   ```csharp
   new FileUploadOptions { AutoUpload = true }
   ```

3. **Manual Upload für Batch-Operations**
   ```csharp
   new FileUploadOptions { AutoUpload = false }
   ```

### ⚠️ Zu beachten

- Clipboard-Bilder haben automatisch generierte Dateinamen
- Bei AutoUpload=false erscheinen Clipboard-Bilder in Pending Files
- Alle Dateien (inkl. Clipboard) durchlaufen die gleiche Validierung

### ❌ Vermeiden

- Keine Storage Service UND großes Volumen (alles in Memory)
- Erwartung eines spezifischen Dateinamens für Clipboard-Bilder

## Migration von v1.0 zu v2.0

### Breaking Changes

**v1.0 Verhalten:**
```csharp
// Clipboard-Bilder wurden IMMER in Files hinzugefügt
// Ignorierte AutoUpload Setting
// Nie durch Storage Service
```

**v2.0 Verhalten:**
```csharp
// Clipboard-Bilder durchlaufen ProcessFiles/StageFiles
// Respektiert AutoUpload Setting
// Verwendet Storage Service wenn vorhanden
```

### Wenn Sie v1.0 Verhalten benötigen:

Implementieren Sie einen Custom Processor:

```csharp
private async Task<FileUploadItem> LegacyClipboardProcessor(IBrowserFile file)
{
    if (file is ClipboardBrowserFile clipboardFile)
    {
        // V1.0 Verhalten: Immer in Memory
        using var stream = file.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        return new FileUploadItem
        {
            FileName = file.Name,
            FileData = ms.ToArray(),
            ContentType = file.ContentType,
            FileSize = ms.Length
        };
    }

    // Normale Dateien wie gewohnt
    return await DefaultProcessing(file);
}
```

## Zusammenfassung

| Feature | Status |
|---------|--------|
| Konsistente Verarbeitung | ✅ Ja |
| Storage Service Support | ✅ Ja |
| AutoUpload Support | ✅ Ja |
| Manual Upload Support | ✅ Ja |
| Pending Files Support | ✅ Ja |
| Custom Processor Support | ✅ Ja |
| Gleiche Validierung | ✅ Ja |
| Gleiche Events | ✅ Ja |

**🎉 Alle Dateien werden gleich behandelt - egal ob aus File Picker, Drag&Drop oder Zwischenablage!**


## Verhalten

### Speicherung

**Clipboard-Bilder werden IMMER im Memory gespeichert**, unabhängig davon, ob ein Storage Service konfiguriert ist:

```csharp
var fileItem = new FileUploadItem
{
    FileName = "clipboard-image-20240115-143022.png",
    FileData = imageBytes,  // ← Die tatsächlichen Bilddaten (nicht ein Filename)
    ContentType = "image/png",
    ThumbnailUrl = base64Data
};
```

### Unterschied zu normalen Uploads

| Eigenschaft | Normale Dateien (mit Storage) | Clipboard-Bilder |
|-------------|------------------------------|------------------|
| Speicherort | Storage Service (Disk/Cloud) | Memory (RAM) |
| `FileData` Inhalt | Filename als UTF8-String | Komplette Bilddaten |
| `FileData` Größe | Klein (~50 Bytes) | Groß (Bildgröße) |
| Persistent | Ja (nach Neustart verfügbar) | Nein (verloren nach Neustart) |
| Upload erforderlich | Optional (Storage Service) | Immer (nur Memory) |

### Löschen

Die `DeleteFile` Methode prüft automatisch, ob die Datei im Storage Service liegt:

```csharp
private async Task DeleteFile(FileUploadItem file)
{
    if (StorageService != null)
    {
        // Prüfe, ob Datei im Storage liegt
        bool existsInStorage = await StorageService.FileExistsAsync(file);
        
        if (existsInStorage)
        {
            // Nur aus Storage löschen, wenn sie dort existiert
            await StorageService.DeleteFileAsync(file);
        }
        // Sonst: Nur Memory, nichts zu löschen
    }
    
    Files.Remove(file); // Aus Memory entfernen
}
```

### Storage Service Implementierung

Der `LocalFileStorageService` erkennt automatisch, ob `FileData` einen Filename oder Bilddaten enthält:

```csharp
private string GetFilePath(FileUploadItem fileItem)
{
    // Wenn FileData > 1KB ist, sind es Bilddaten, kein Filename
    if (fileItem.FileData.Length > 1024)
    {
        throw new InvalidOperationException("FileData contains file content, not filename");
    }
    
    var uniqueFileName = System.Text.Encoding.UTF8.GetString(fileItem.FileData);
    return Path.Combine(_basePath, uniqueFileName);
}
```

`FileExistsAsync` fängt diese Exception ab und gibt `false` zurück:

```csharp
public Task<bool> FileExistsAsync(FileUploadItem fileItem)
{
    try
    {
        var filePath = GetFilePath(fileItem);
        return Task.FromResult(File.Exists(filePath));
    }
    catch
    {
        // FileData enthält keine Filename → nicht im Storage
        return Task.FromResult(false);
    }
}
```

## Use Cases

### 1. Clipboard-Bild einfügen und sofort löschen

```razor
<FileUploadManager @bind-Files="files" />
```

**Ablauf:**
1. Benutzer drückt Ctrl+V → Bild wird in Memory gespeichert
2. Benutzer klickt "Delete" → Bild wird nur aus Memory entfernt
3. ✅ Kein Fehler, da `FileExistsAsync` → `false`

### 2. Clipboard-Bild persistent speichern

Wenn Sie möchten, dass Clipboard-Bilder auch in den Storage Service gespeichert werden:

```csharp
// Option 1: Nach dem Einfügen manuell speichern
private async Task SaveClipboardImageToStorage(FileUploadItem clipboardImage)
{
    if (clipboardImage.FileData != null && clipboardImage.FileData.Length > 1024)
    {
        // Erstelle einen Mock IBrowserFile
        var mockFile = new MockBrowserFile(
            clipboardImage.FileName, 
            clipboardImage.FileData, 
            clipboardImage.ContentType
        );
        
        // Speichere in Storage Service
        var storedFile = await StorageService.SaveFileAsync(mockFile);
        
        // Ersetze Memory-Version mit Storage-Version
        files.Remove(clipboardImage);
        files.Add(storedFile);
    }
}
```

Oder verwenden Sie `AutoUpload = false` und den Upload-Button.

### 3. AutoUpload mit Storage Service

**Aktuelles Verhalten:**
```razor
<FileUploadManager 
    StorageService="@storageService"
    Options="@(new FileUploadOptions { AutoUpload = true })" />
```

- Normale Dateien: Werden sofort in Storage gespeichert ✅
- Clipboard-Bilder: Bleiben im Memory ⚠️

**Geplante Verbesserung:**
In zukünftigen Versionen könnten Clipboard-Bilder auch automatisch in den Storage Service hochgeladen werden.

## Best Practices

### ✅ Empfohlen

1. **Für temporäre Bilder:** Clipboard-Funktion verwenden (bleibt in Memory)
2. **Für persistente Uploads:** Dateien per Browse/Drag&Drop hochladen
3. **Mixed Use:** Clipboard + AutoUpload=false, dann Upload-Button verwenden

### ⚠️ Zu beachten

- Clipboard-Bilder gehen bei Neustart verloren (nur Memory)
- Bei großen Bildern kann viel RAM verwendet werden
- Download funktioniert auch für Clipboard-Bilder (aus Memory)

### ❌ Vermeiden

- Viele große Clipboard-Bilder ohne Upload (RAM-Problem)
- Erwartung, dass Clipboard-Bilder persistent sind ohne Storage

## Zusammenfassung

| Feature | Status |
|---------|--------|
| Clipboard einfügen | ✅ Funktioniert |
| Im Memory speichern | ✅ Automatisch |
| In Storage speichern | ⚠️ Manuell erforderlich |
| Löschen aus Memory | ✅ Funktioniert |
| Löschen aus Storage | ✅ Wird korrekt übersprungen |
| Download aus Memory | ✅ Funktioniert |
| Preview | ✅ Funktioniert |

## Technische Details

### FileUploadItem Unterscheidung

So können Sie programmatisch erkennen, ob eine Datei im Storage oder Memory liegt:

```csharp
bool IsInMemory(FileUploadItem file)
{
    // Memory-Dateien haben große FileData (> 1KB)
    return file.FileData != null && file.FileData.Length > 1024;
}

bool IsInStorage(FileUploadItem file)
{
    // Storage-Dateien haben kleine FileData (Filename als String)
    return file.FileData != null && file.FileData.Length <= 1024;
}

bool IsClipboardImage(FileUploadItem file)
{
    return file.FileName.StartsWith("clipboard-image-") && IsInMemory(file);
}
```

### Migration von Memory zu Storage

```csharp
public async Task MigrateToStorage(FileUploadItem memoryFile)
{
    if (!IsInMemory(memoryFile))
        return; // Bereits im Storage

    // Erstelle temporäre Datei
    var tempPath = Path.GetTempFileName();
    await File.WriteAllBytesAsync(tempPath, memoryFile.FileData);

    // Erstelle Mock IBrowserFile
    var mockFile = new MockBrowserFile(memoryFile.FileName, tempPath);

    // Speichere in Storage
    var storageFile = await StorageService.SaveFileAsync(mockFile);

    // Ersetze in Liste
    var index = Files.IndexOf(memoryFile);
    Files[index] = storageFile;

    // Cleanup
    File.Delete(tempPath);
}
```
