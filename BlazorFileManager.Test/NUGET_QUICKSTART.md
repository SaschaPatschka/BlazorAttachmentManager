# 🚀 Schnellstart: NuGet-Paket veröffentlichen

## ✅ Paket erfolgreich erstellt!

Dein NuGet-Paket wurde erstellt: `BlazorFileManager.2.0.0.nupkg` (65 KB)
Speicherort: `C:\Users\patsc\source\repos\BlazorAttachmentManager\BlazorFileManager\nupkg\`

## Nächste Schritte

### 1. NuGet.org Account vorbereiten

1. Gehe zu [nuget.org](https://www.nuget.org/) und melde dich an (oder erstelle einen Account)
2. Navigiere zu deinem Account → **API Keys**
3. Klicke auf **Create** um einen neuen API-Key zu erstellen
4. Gib einen Namen ein (z.B. "BlazorFileManager")
5. Wähle unter **Select Scopes**: **Push** und **Push new packages and package versions**
6. Wähle **Select Packages**: **Glob Pattern** und gib `BlazorFileManager` ein
7. Klicke auf **Create**
8. **WICHTIG**: Kopiere den API-Key sofort (er wird nur einmal angezeigt!)

### 2. Paket auf NuGet.org veröffentlichen

Öffne PowerShell im Verzeichnis `BlazorFileManager` und führe aus:

```powershell
dotnet nuget push ./nupkg/BlazorFileManager.2.0.0.nupkg --api-key <DEIN-API-KEY> --source https://api.nuget.org/v3/index.json
```

**Ersetze `<DEIN-API-KEY>`** mit dem kopierten API-Key!

### 3. Warte auf Veröffentlichung

- Nach dem Push dauert es 5-10 Minuten bis das Paket verfügbar ist
- Du kannst den Status unter [nuget.org/packages/BlazorFileManager](https://www.nuget.org/packages/BlazorFileManager) prüfen
- Du erhältst eine E-Mail-Bestätigung, wenn das Paket veröffentlicht wurde

### 4. Paket testen

Nach der Veröffentlichung können andere (und du) das Paket installieren:

```bash
dotnet add package BlazorFileManager
```

## 🔄 Updates veröffentlichen

Wenn du später eine neue Version veröffentlichen möchtest:

1. **Version erhöhen** in `BlazorFileManager.csproj`:
   ```xml
   <Version>2.0.1</Version>
   <PackageReleaseNotes>Bug fix: Thumbnail preview for storage-based files</PackageReleaseNotes>
   ```

2. **Neu erstellen und pushen**:
   ```powershell
   dotnet pack -c Release -o ./nupkg
   dotnet nuget push ./nupkg/BlazorFileManager.2.0.1.nupkg --api-key <API-KEY> --source https://api.nuget.org/v3/index.json
   ```

## 📋 Versionierung (Semantic Versioning)

- **Major (3.0.0)**: Breaking Changes - API-Änderungen, die bestehenden Code brechen
- **Minor (2.1.0)**: Neue Features - abwärtskompatibel
- **Patch (2.0.1)**: Bug-Fixes - abwärtskompatibel

Aktuelle Version: **2.0.0** (Major Release mit unified upload pipeline)

## 🛡️ Sicherheit

- Speichere deinen API-Key **NIEMALS** in Git/Code
- Verwende Environment Variables oder Secrets für automatisierte Builds
- Erstelle separate API-Keys für verschiedene Projekte

## 📚 Weitere Informationen

Siehe `NUGET_PUBLISHING.md` für:
- GitHub Actions Automatisierung
- Symbol Packages (.pdb)
- Erweiterte NuGet-Konfiguration
- Troubleshooting

## 🎉 Nach der Veröffentlichung

Füge ein Badge zu deinem `README.md` hinzu:

```markdown
[![NuGet](https://img.shields.io/nuget/v/BlazorFileManager.svg)](https://www.nuget.org/packages/BlazorFileManager/)
[![Downloads](https://img.shields.io/nuget/dt/BlazorFileManager.svg)](https://www.nuget.org/packages/BlazorFileManager/)
```

## ⚠️ Wichtige Hinweise

1. **Paket-Name ist permanent**: Du kannst `BlazorFileManager` nicht mehr ändern nach der ersten Veröffentlichung
2. **Versionen sind unveränderlich**: Eine veröffentlichte Version (z.B. 2.0.0) kann nicht überschrieben werden
3. **Löschung ist kompliziert**: Pakete können nur "unlisted" werden, nicht komplett gelöscht
4. **README wird automatisch angezeigt**: Die `NUGET_README.md` erscheint auf der NuGet-Paketseite

## 🆘 Support

Bei Problemen:
- [NuGet Documentation](https://docs.microsoft.com/nuget/)
- [NuGet Support](https://www.nuget.org/policies/Contact)
- Erstelle ein Issue im GitHub Repository

Viel Erfolg! 🚀
