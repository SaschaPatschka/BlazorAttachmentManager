# Changelog - Version 2.0

## Major Changes

### ✨ Konsistente Dateiverarbeitung (Breaking Change)

**Alle Dateiquellen werden jetzt gleich behandelt!**

Clipboard-Bilder (Ctrl+V) durchlaufen jetzt die gleiche Upload-Pipeline wie normale Dateien:

#### Vorher (v1.x):
```csharp
// Clipboard-Bilder
- ❌ Immer sofort in Files hinzugefügt
- ❌ Ignorierte AutoUpload Setting
- ❌ Nie durch Storage Service
- ❌ Spezielle Delete-Logik erforderlich

// Normale Dateien
- ✅ Respektierten AutoUpload
- ✅ Durch Storage Service
- ✅ Standard Delete-Logik
```

#### Jetzt (v2.0):
```csharp
// ALLE Dateien (File Picker, Drag&Drop, Clipboard)
- ✅ Gleiche Upload-Pipeline
- ✅ Respektieren AutoUpload
- ✅ Durch Storage Service (wenn konfiguriert)
- ✅ Gleiche Validierung
- ✅ Gleiche Events
- ✅ Gleiche Delete-Logik
```

### Neue Klasse: ClipboardBrowserFile

```csharp
internal class ClipboardBrowserFile : IBrowserFile
{
    // Konvertiert Clipboard-Daten in IBrowserFile
    // Ermöglicht konsistente Verarbeitung
}
```

### Verhalten mit Storage Service

**AutoUpload = true:**
```csharp
<FileUploadManager 
    StorageService="@storageService"
    Options="@(new { AutoUpload = true })" />
```
- Ctrl+V → Bild wird **sofort** über Storage Service gespeichert ✅
- Persistent auf Disk/Cloud
- Gleich wie normale Dateien

**AutoUpload = false:**
```csharp
<FileUploadManager 
    StorageService="@storageService"
    Options="@(new { AutoUpload = false })" />
```
- Ctrl+V → Bild erscheint in **Pending Files** ✅
- Upload-Button speichert über Storage Service
- Gleich wie normale Dateien

## Migration Guide

### Wenn Sie Storage Service verwenden:

**Keine Änderung erforderlich!** 

Clipboard-Bilder werden jetzt automatisch über den Storage Service gespeichert - genau wie Sie es wahrscheinlich erwartet haben.

### Wenn Sie KEIN Storage Service verwenden:

**Keine Änderung erforderlich!**

Clipboard-Bilder bleiben im Memory (wie vorher), aber durchlaufen jetzt die gleiche Pipeline.

### Wenn Sie Custom Processing verwenden:

Ihr `CustomFileProcessor` wird **jetzt auch für Clipboard-Bilder** aufgerufen:

```csharp
private async Task<FileUploadItem> MyProcessor(IBrowserFile file)
{
    // NEU: Wird auch für Clipboard-Bilder aufgerufen
    if (file is ClipboardBrowserFile clipboardFile)
    {
        // Optional: Spezielle Behandlung
        Console.WriteLine("Clipboard image detected");
    }
    
    // Normale Verarbeitung
    return await ProcessFile(file);
}
```

### Breaking Changes

1. **Clipboard-Bilder respektieren jetzt AutoUpload**
   - Vorher: Immer sofort hochgeladen
   - Jetzt: Bei AutoUpload=false in Pending Files

2. **Clipboard-Bilder verwenden Storage Service**
   - Vorher: Immer nur Memory
   - Jetzt: Über Storage Service wenn konfiguriert

3. **OnFileUploaded Event Timing**
   - Vorher: Sofort nach Clipboard-Paste
   - Jetzt: Nach tatsächlichem Upload (bei AutoUpload=false erst nach Button-Click)

## Vorteile

### ✅ Konsistenz
- Eine Code-Path für alle Datei-Quellen
- Einfacheres Verständnis und Wartung

### ✅ Flexibilität
- AutoUpload funktioniert für alle Dateien
- Storage Service für alle Dateien
- Custom Processing für alle Dateien

### ✅ Vorhersagbarkeit
- Gleiche Validierung überall
- Gleiche Events überall
- Gleiche Error Handling überall

### ✅ Best Practices
- Interface-basiert (`IBrowserFile`)
- Dependency Injection (`IFileStorageService`)
- Single Responsibility (eine Upload-Pipeline)

## Beispiele

### Vor v2.0:
```csharp
// Clipboard → Memory (immer)
// File Upload → Storage (wenn konfiguriert)
// Unterschiedliches Verhalten je nach Quelle
```

### Ab v2.0:
```csharp
// ALLE Quellen → Storage (wenn konfiguriert)
// ALLE Quellen → respektieren AutoUpload
// ALLE Quellen → gleiche Pipeline
```

## Testen

**Szenario 1: Mit Storage Service**
1. Füge Storage Service hinzu: `builder.Services.AddFileManagerLocalStorage()`
2. Füge Bild aus Zwischenablage ein (Ctrl+V)
3. ✅ Prüfe: Datei liegt im Upload-Ordner
4. Lösche Datei
5. ✅ Prüfe: Datei aus Ordner gelöscht

**Szenario 2: Ohne Storage Service**
1. Kein Storage Service konfiguriert
2. Füge Bild aus Zwischenablage ein (Ctrl+V)
3. ✅ Prüfe: Datei in Files-Liste
4. Lösche Datei
5. ✅ Prüfe: Kein Fehler

**Szenario 3: AutoUpload=false**
1. Storage Service konfiguriert
2. AutoUpload = false
3. Füge Bild aus Zwischenablage ein (Ctrl+V)
4. ✅ Prüfe: Bild in Pending Files
5. Klicke Upload Button
6. ✅ Prüfe: Datei im Upload-Ordner

## Weitere Änderungen

### Bug Fixes
- ✅ Löschen von Clipboard-Bildern funktioniert ohne Fehler
- ✅ FileExistsAsync prüft korrekt ob Datei im Storage liegt
- ✅ GetFilePath erkennt Memory vs. Storage Dateien

### Verbesserungen
- ✅ Bessere Fehlerbehandlung
- ✅ Klarere Dokumentation
- ✅ Konsistentere API

## Rückwärts-Kompatibilität

### Kompatibel:
- ✅ File Upload via Browse/Drag&Drop
- ✅ Storage Service Interface
- ✅ FileUploadOptions
- ✅ Events (OnFileUploaded, OnFileDeleted, etc.)
- ✅ Templates (alle)
- ✅ Labels/Multi-Language

### Potenziell Breaking:
- ⚠️ Clipboard-Bilder Timing (bei AutoUpload=false)
- ⚠️ CustomFileProcessor (erhält jetzt auch Clipboard-Bilder)

## Version Info

- **Version:** 2.0.0
- **Datum:** Januar 2024
- **Breaking Changes:** Ja (Clipboard-Verhalten)
- **Migration Required:** Minimal (nur bei Custom Processing)
