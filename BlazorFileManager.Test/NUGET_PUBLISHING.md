# NuGet Package Publishing Guide

## Voraussetzungen

1. **NuGet Account**: 
   - Erstelle einen Account auf [nuget.org](https://www.nuget.org/)
   - Erstelle einen API-Key unter [API Keys](https://www.nuget.org/account/apikeys)
   - Wähle "Push" permission und gebe einen Namen ein (z.B. "BlazorFileManager")

2. **.NET SDK**: 
   - .NET 8.0 SDK installiert

## Schritt 1: Paket erstellen

Öffne PowerShell im Projektverzeichnis `BlazorFileManager`:

```powershell
# Build im Release-Modus
dotnet build -c Release

# NuGet-Paket erstellen
dotnet pack -c Release -o ./nupkg
```

Das erstellt die Datei `BlazorFileManager.2.0.0.nupkg` im Ordner `./nupkg`.

## Schritt 2: Paket lokal testen (Optional)

Bevor du veröffentlichst, kannst du das Paket lokal testen:

```powershell
# Füge eine lokale NuGet-Quelle hinzu
dotnet nuget add source C:\Users\patsc\source\repos\BlazorAttachmentManager\BlazorFileManager\nupkg --name LocalBlazorFileManager

# Im Test-Projekt: Installiere das lokale Paket
dotnet add package BlazorFileManager --version 2.0.0 --source LocalBlazorFileManager
```

## Schritt 3: Auf NuGet.org veröffentlichen

```powershell
# Mit API-Key veröffentlichen
dotnet nuget push ./nupkg/BlazorFileManager.2.0.0.nupkg --api-key <DEIN-API-KEY> --source https://api.nuget.org/v3/index.json
```

**Wichtig**: Ersetze `<DEIN-API-KEY>` mit deinem tatsächlichen API-Key von nuget.org.

## Schritt 4: Paket auf NuGet.org verifizieren

Nach ca. 5-10 Minuten ist das Paket verfügbar:
- Suche auf [nuget.org](https://www.nuget.org/packages/BlazorFileManager)
- Installation mit: `dotnet add package BlazorFileManager`

## Neue Version veröffentlichen

Wenn du Updates veröffentlichen möchtest:

1. **Version aktualisieren** in `BlazorFileManager.csproj`:
   ```xml
   <Version>2.1.0</Version>
   <PackageReleaseNotes>Bug fixes and improvements</PackageReleaseNotes>
   ```

2. **Paket neu erstellen und pushen**:
   ```powershell
   dotnet pack -c Release -o ./nupkg
   dotnet nuget push ./nupkg/BlazorFileManager.2.1.0.nupkg --api-key <API-KEY> --source https://api.nuget.org/v3/index.json
   ```

## Versionierung (Semantic Versioning)

- **Major (2.0.0)**: Breaking Changes (z.B. API-Änderungen)
- **Minor (2.1.0)**: Neue Features (abwärtskompatibel)
- **Patch (2.0.1)**: Bug-Fixes (abwärtskompatibel)

## GitHub Actions (Automatisches Publishing)

Für automatisches Publishing bei jedem Git-Tag kannst du GitHub Actions verwenden.

Erstelle `.github/workflows/publish-nuget.yml`:

```yaml
name: Publish NuGet Package

on:
  push:
    tags:
      - 'v*.*.*'

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build -c Release --no-restore
      
      - name: Pack
        run: dotnet pack BlazorFileManager/BlazorFileManager.csproj -c Release -o ./nupkg --no-build
      
      - name: Push to NuGet
        run: dotnet nuget push ./nupkg/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json
```

**Setup**:
1. Gehe zu GitHub Repository → Settings → Secrets → Actions
2. Erstelle ein neues Secret `NUGET_API_KEY` mit deinem NuGet API-Key
3. Veröffentliche mit Git-Tag:
   ```powershell
   git tag v2.0.0
   git push origin v2.0.0
   ```

## NuGet Package Badge

Füge ein Badge zu deinem README hinzu:

```markdown
[![NuGet](https://img.shields.io/nuget/v/BlazorFileManager.svg)](https://www.nuget.org/packages/BlazorFileManager/)
[![Downloads](https://img.shields.io/nuget/dt/BlazorFileManager.svg)](https://www.nuget.org/packages/BlazorFileManager/)
```

## Troubleshooting

### Fehler: "Package already exists"
- Du kannst die gleiche Version nicht zweimal pushen
- Erhöhe die Version in `.csproj`

### Fehler: "Unauthorized"
- Prüfe deinen API-Key
- Stelle sicher, dass der API-Key "Push" permission hat

### Paket erscheint nicht in der Suche
- Warte 10-15 Minuten nach dem Upload
- Prüfe unter "Manage Packages" in deinem nuget.org Profil

### Symbole (.pdb) mitveröffentlichen

Für besseres Debugging:

```xml
<PropertyGroup>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>
```

Dann:
```powershell
dotnet pack -c Release -o ./nupkg --include-symbols
```

## Weitere Ressourcen

- [NuGet Documentation](https://docs.microsoft.com/nuget/)
- [Packaging Best Practices](https://docs.microsoft.com/nuget/create-packages/package-authoring-best-practices)
- [NuGet API Keys](https://www.nuget.org/account/apikeys)
