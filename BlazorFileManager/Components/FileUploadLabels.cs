namespace BlazorFileManager.Components;

/// <summary>
/// Contains all text labels used in the FileUploadManager component.
/// Create a custom instance to support different languages.
/// </summary>
public class FileUploadLabels
{
    // Dropzone
    public string DropzoneText { get; set; } = "Drag & Drop files here or click to browse";
    public string ClipboardHint { get; set; } = "You can also paste images from clipboard (Ctrl+V)";
    public string ClipboardButtonText { get; set; } = "📋 Paste from Clipboard";

    // Pending Files
    public string PendingFilesTitle { get; set; } = "📋 Pending Files";
    public string ClearAllButton { get; set; } = "Clear All";
    public string UploadButton { get; set; } = "⬆️ Upload {0} File(s)";
    public string UploadingText { get; set; } = "⏳ Uploading...";
    public string RemoveFileButton { get; set; } = "✕";

    // File List
    public string DownloadButton { get; set; } = "⬇️ Download";
    public string DeleteButton { get; set; } = "🗑️ Delete";

    // Header
    public string FilesCountText { get; set; } = "{0} / {1} files";

    // Error Messages
    public string ErrorMaxFilesReached { get; set; } = "Maximum number of files ({0}) reached.";
    public string ErrorFileTooLarge { get; set; } = "File '{0}' exceeds maximum size of {1}.";
    public string ErrorFileTypeNotAllowed { get; set; } = "File type '{0}' is not allowed for file '{1}'.";
    public string ErrorNoFilesToUpload { get; set; } = "No files to upload.";
    public string ErrorUploadInProgress { get; set; } = "Upload already in progress.";
    public string ErrorInitialization { get; set; } = "Initialization error: {0}";
    public string ErrorJavaScriptModule { get; set; } = "JavaScript module is loading... Please try again in a moment.";
    public string ErrorComponentNotInitialized { get; set; } = "Component not initialized. Please reload the page.";
    public string ErrorJavaScript { get; set; } = "JavaScript error: {0}";
    public string ErrorReadingClipboard { get; set; } = "Error reading clipboard: {0}";
    public string ErrorNoImageData { get; set; } = "No image data received.";
    public string ErrorDecodingImage { get; set; } = "Error decoding image: {0}";
    public string ErrorMaxFilesReachedClipboard { get; set; } = "Maximum number of files ({0}) reached.";
    public string ErrorImageTooLarge { get; set; } = "Image exceeds maximum size of {0}.";
    public string ErrorProcessingClipboardImage { get; set; } = "Error processing clipboard image: {0}";
    public string ErrorDeletingFile { get; set; } = "Failed to delete file '{0}' from storage.";
    public string ErrorDeletingFileException { get; set; } = "Error deleting file '{0}': {1}";
    public string ErrorDownloadingFile { get; set; } = "Error downloading file '{0}': {1}";
    
    // Compression Messages
    public string ImageCompressed { get; set; } = "✓ Image '{0}' was compressed: {1} → {2}";
    public string ImageCompressionFailed { get; set; } = "Image '{0}' could not be compressed sufficiently. {1}";
    public string ErrorDuringCompression { get; set; } = "Error compressing '{0}': {1}";

    /// <summary>
    /// Creates labels for German language
    /// </summary>
    public static FileUploadLabels German => new()
    {
        DropzoneText = "Dateien hier ablegen oder klicken zum Durchsuchen",
        ClipboardHint = "Sie können auch Bilder aus der Zwischenablage einfügen (Strg+V)",
        ClipboardButtonText = "📋 Aus Zwischenablage einfügen",
        
        PendingFilesTitle = "📋 Ausstehende Dateien",
        ClearAllButton = "Alle löschen",
        UploadButton = "⬆️ {0} Datei(en) hochladen",
        UploadingText = "⏳ Wird hochgeladen...",
        RemoveFileButton = "✕",
        
        DownloadButton = "⬇️ Herunterladen",
        DeleteButton = "🗑️ Löschen",
        
        FilesCountText = "{0} / {1} Dateien",
        
        ErrorMaxFilesReached = "Maximale Anzahl von Dateien ({0}) erreicht.",
        ErrorFileTooLarge = "Datei '{0}' überschreitet die maximale Größe von {1}.",
        ErrorFileTypeNotAllowed = "Dateityp '{0}' ist für Datei '{1}' nicht erlaubt.",
        ErrorNoFilesToUpload = "Keine Dateien zum Hochladen.",
        ErrorUploadInProgress = "Upload läuft bereits.",
        ErrorInitialization = "Initialisierungsfehler: {0}",
        ErrorJavaScriptModule = "JavaScript-Modul wird geladen... Bitte versuchen Sie es gleich nochmal.",
        ErrorComponentNotInitialized = "Komponente nicht initialisiert. Bitte Seite neu laden.",
        ErrorJavaScript = "JavaScript-Fehler: {0}",
        ErrorReadingClipboard = "Fehler beim Lesen der Zwischenablage: {0}",
        ErrorNoImageData = "Keine Bilddaten empfangen.",
        ErrorDecodingImage = "Fehler beim Dekodieren des Bildes: {0}",
        ErrorMaxFilesReachedClipboard = "Maximale Anzahl von Dateien ({0}) erreicht.",
        ErrorImageTooLarge = "Bild überschreitet maximale Größe von {0}.",
        ErrorProcessingClipboardImage = "Fehler beim Verarbeiten des Zwischenablage-Bildes: {0}",
        ErrorDeletingFile = "Fehler beim Löschen der Datei '{0}' aus dem Speicher.",
        ErrorDeletingFileException = "Fehler beim Löschen der Datei '{0}': {1}",
        ErrorDownloadingFile = "Fehler beim Herunterladen der Datei '{0}': {1}",
        
        ImageCompressed = "✓ Bild '{0}' wurde komprimiert: {1} → {2}",
        ImageCompressionFailed = "Bild '{0}' konnte nicht ausreichend komprimiert werden. {1}",
        ErrorDuringCompression = "Fehler beim Komprimieren von '{0}': {1}"
    };

    /// <summary>
    /// Creates labels for English language (default)
    /// </summary>
    public static FileUploadLabels English => new();

    /// <summary>
    /// Creates labels for French language
    /// </summary>
    public static FileUploadLabels French => new()
    {
        DropzoneText = "Déposez les fichiers ici ou cliquez pour parcourir",
        ClipboardHint = "Vous pouvez également coller des images depuis le presse-papiers (Ctrl+V)",
        ClipboardButtonText = "📋 Coller depuis le presse-papiers",
        
        PendingFilesTitle = "📋 Fichiers en attente",
        ClearAllButton = "Tout effacer",
        UploadButton = "⬆️ Télécharger {0} fichier(s)",
        UploadingText = "⏳ Téléchargement en cours...",
        RemoveFileButton = "✕",
        
        DownloadButton = "⬇️ Télécharger",
        DeleteButton = "🗑️ Supprimer",
        
        FilesCountText = "{0} / {1} fichiers",
        
        ErrorMaxFilesReached = "Nombre maximum de fichiers ({0}) atteint.",
        ErrorFileTooLarge = "Le fichier '{0}' dépasse la taille maximale de {1}.",
        ErrorFileTypeNotAllowed = "Le type de fichier '{0}' n'est pas autorisé pour le fichier '{1}'.",
        ErrorNoFilesToUpload = "Aucun fichier à télécharger.",
        ErrorUploadInProgress = "Téléchargement déjà en cours.",
        ErrorInitialization = "Erreur d'initialisation: {0}",
        ErrorJavaScriptModule = "Le module JavaScript se charge... Veuillez réessayer dans un instant.",
        ErrorComponentNotInitialized = "Composant non initialisé. Veuillez recharger la page.",
        ErrorJavaScript = "Erreur JavaScript: {0}",
        ErrorReadingClipboard = "Erreur lors de la lecture du presse-papiers: {0}",
        ErrorNoImageData = "Aucune donnée d'image reçue.",
        ErrorDecodingImage = "Erreur lors du décodage de l'image: {0}",
        ErrorMaxFilesReachedClipboard = "Nombre maximum de fichiers ({0}) atteint.",
        ErrorImageTooLarge = "L'image dépasse la taille maximale de {0}.",
        ErrorProcessingClipboardImage = "Erreur lors du traitement de l'image du presse-papiers: {0}",
        ErrorDeletingFile = "Échec de la suppression du fichier '{0}' du stockage.",
        ErrorDeletingFileException = "Erreur lors de la suppression du fichier '{0}': {1}",
        ErrorDownloadingFile = "Erreur lors du téléchargement du fichier '{0}': {1}",
        
        ImageCompressed = "✓ L'image '{0}' a été compressée: {1} → {2}",
        ImageCompressionFailed = "L'image '{0}' n'a pas pu être suffisamment compressée. {1}",
        ErrorDuringCompression = "Erreur lors de la compression de '{0}': {1}"
    };
}
