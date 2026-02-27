using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.IO;
using BlazorFileManager.Services;

namespace BlazorFileManager.Components;

public partial class FileUploadManager : ComponentBase, IAsyncDisposable
{
    private ElementReference containerElement;
    private string inputId = $"file-input-{Guid.NewGuid()}";
    private bool isDragging = false;
    private DotNetObjectReference<FileUploadManager>? dotNetHelper;
    private IJSObjectReference? jsModule;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Optional file storage service for saving files to persistent storage.
    /// If provided, files will be saved using this service instead of storing in memory.
    /// </summary>
    [Parameter]
    public IFileStorageService? StorageService { get; set; }

    /// <summary>
    /// Title displayed in the header
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "File Manager";

    /// <summary>
    /// Configuration options for the file upload manager
    /// </summary>
    [Parameter]
    public FileUploadOptions Options { get; set; } = new();

    /// <summary>
    /// List of uploaded files
    /// </summary>
    [Parameter]
    public List<FileUploadItem> Files { get; set; } = new();

    /// <summary>
    /// Event callback when files are uploaded
    /// </summary>
    [Parameter]
    public EventCallback<List<FileUploadItem>> FilesChanged { get; set; }

    /// <summary>
    /// Event callback when a file is uploaded
    /// </summary>
    [Parameter]
    public EventCallback<FileUploadItem> OnFileUploaded { get; set; }

    /// <summary>
    /// Event callback when a file is deleted
    /// </summary>
    [Parameter]
    public EventCallback<FileUploadItem> OnFileDeleted { get; set; }

    /// <summary>
    /// Event callback when a file is downloaded
    /// </summary>
    [Parameter]
    public EventCallback<FileUploadItem> OnFileDownloaded { get; set; }

    /// <summary>
    /// Custom template for the header section
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Custom template for the drop zone
    /// </summary>
    [Parameter]
    public RenderFragment? DropZoneTemplate { get; set; }

    /// <summary>
    /// Custom template for each file item
    /// </summary>
    [Parameter]
    public RenderFragment<FileUploadItem>? FileItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template used to render the button for pasting content from the clipboard.
    /// </summary>
    /// <remarks>Set this property to customize the appearance and behavior of the paste-from-clipboard
    /// button. If not specified, a default button template is used.</remarks>
    [Parameter]
    public RenderFragment? PasteFromClipboardButtonTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template used to render the upload button when AutoUpload is disabled.
    /// </summary>
    /// <remarks>Set this property to customize the appearance and behavior of the upload button.
    /// If not specified, a default button template is used.</remarks>
    [Parameter]
    public RenderFragment? UploadButtonTemplate { get; set; }

    /// <summary>
    /// Custom template for the footer section
    /// </summary>
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Custom file processing logic. If not provided, files are stored in memory.
    /// </summary>
    [Parameter]
    public Func<IBrowserFile, Task<FileUploadItem>>? CustomFileProcessor { get; set; }

    /// <summary>
    /// Text labels for the component. Use this to provide translations or custom text.
    /// Default is English. Use FileUploadLabels.German or FileUploadLabels.French for other languages,
    /// or create your own instance.
    /// </summary>
    [Parameter]
    public FileUploadLabels Labels { get; set; } = FileUploadLabels.English;

    private List<string> ErrorMessages { get; set; } = new();
    private List<IBrowserFile> PendingFiles { get; set; } = new();
    private bool isUploading = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorFileManager/fileUploadManager.js");

                if (Options.AllowPasteFromClipboard)
                {
                    dotNetHelper = DotNetObjectReference.Create(this);
                    await jsModule.InvokeVoidAsync("initializePasteHandler", 
                        dotNetHelper, $"paste-area-{inputId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing FileUploadManager: {ex.Message}");
                ErrorMessages.Add(string.Format(Labels.ErrorInitialization, ex.Message));
            }
        }
    }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        ErrorMessages.Clear();

        var files = e.GetMultipleFiles(Options.AllowMultiple ? int.MaxValue : 1);

        if (Options.AutoUpload)
        {
            // Immediate upload
            await ProcessFiles(files);
        }
        else
        {
            // Stage files for later upload
            StageFiles(files);
        }
    }

    private void StageFiles(IEnumerable<IBrowserFile> browserFiles)
    {
        foreach (var browserFile in browserFiles)
        {
            // Validate file count
            if (Options.MaxFileCount > 0 && (Files.Count + PendingFiles.Count) >= Options.MaxFileCount)
            {
                ErrorMessages.Add(string.Format(Labels.ErrorMaxFilesReached, Options.MaxFileCount));
                break;
            }

            // Validate file size
            if (browserFile.Size > Options.MaxFileSize)
            {
                ErrorMessages.Add(string.Format(Labels.ErrorFileTooLarge, browserFile.Name, FormatBytes(Options.MaxFileSize)));
                continue;
            }

            // Validate file type
            if (Options.AllowedFileTypes.Any() && !Options.AllowedFileTypes.Contains(browserFile.ContentType))
            {
                ErrorMessages.Add(string.Format(Labels.ErrorFileTypeNotAllowed, browserFile.ContentType, browserFile.Name));
                continue;
            }

            PendingFiles.Add(browserFile);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Manually uploads all pending files. Call this method when AutoUpload is disabled.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task UploadFilesAsync()
    {
        if (isUploading)
        {
            ErrorMessages.Add(Labels.ErrorUploadInProgress);
            return;
        }

        if (!PendingFiles.Any())
        {
            ErrorMessages.Add(Labels.ErrorNoFilesToUpload);
            return;
        }

        isUploading = true;
        StateHasChanged();

        try
        {
            await ProcessFiles(PendingFiles);
            PendingFiles.Clear();
        }
        finally
        {
            isUploading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Removes a pending file from the upload queue
    /// </summary>
    /// <param name="file">The file to remove</param>
    public void RemovePendingFile(IBrowserFile file)
    {
        PendingFiles.Remove(file);
        StateHasChanged();
    }

    /// <summary>
    /// Clears all pending files
    /// </summary>
    public void ClearPendingFiles()
    {
        PendingFiles.Clear();
        StateHasChanged();
    }

    private async Task ProcessFiles(IEnumerable<IBrowserFile> browserFiles)
    {
        foreach (var browserFile in browserFiles)
        {
            try
            {
                // Validate file count
                if (Options.MaxFileCount > 0 && Files.Count >= Options.MaxFileCount)
                {
                    ErrorMessages.Add(string.Format(Labels.ErrorMaxFilesReached, Options.MaxFileCount));
                    break;
                }

                // Check if file is an image and auto-compression is enabled
                bool isImage = browserFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
                bool needsCompression = isImage && 
                                       Options.AutoCompressImages && 
                                       browserFile.Size > Options.MaxFileSize;

                // Validate file size (unless it's an image that will be compressed)
                if (!needsCompression && browserFile.Size > Options.MaxFileSize)
                {
                    ErrorMessages.Add(string.Format(Labels.ErrorFileTooLarge, browserFile.Name, FormatBytes(Options.MaxFileSize)));
                    continue;
                }

                // Validate file type
                if (Options.AllowedFileTypes.Any() && !Options.AllowedFileTypes.Contains(browserFile.ContentType))
                {
                    ErrorMessages.Add(string.Format(Labels.ErrorFileTypeNotAllowed, browserFile.ContentType, browserFile.Name));
                    continue;
                }

                FileUploadItem fileItem;

                if (CustomFileProcessor != null)
                {
                    // Use custom processor
                    fileItem = await CustomFileProcessor(browserFile);
                }
                else if (StorageService != null)
                {
                    // Use storage service to save file
                    fileItem = await StorageService.SaveFileAsync(browserFile);

                    // Generate thumbnail for images by loading data from storage
                    if (fileItem.IsImage)
                    {
                        try
                        {
                            var imageBytes = await StorageService.GetFileBytesAsync(fileItem);
                            if (imageBytes != null && imageBytes.Length > 0)
                            {
                                var base64 = Convert.ToBase64String(imageBytes);
                                fileItem.ThumbnailUrl = $"data:{fileItem.ContentType};base64,{base64}";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading thumbnail for {fileItem.FileName}: {ex.Message}");
                            // ThumbnailUrl remains null, default icon will be used
                        }
                    }
                }
                else
                {
                    // Default processing: read file into memory
                    using var stream = browserFile.OpenReadStream(Options.MaxFileSize * 10); // Allow larger for compression
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);

                    var fileData = memoryStream.ToArray();
                    var base64 = Convert.ToBase64String(fileData);
                    var dataUrl = $"data:{browserFile.ContentType};base64,{base64}";

                    // Try to compress if needed
                    if (needsCompression && jsModule != null)
                    {
                        try
                        {
                            Console.WriteLine($"Attempting to compress {browserFile.Name} ({FormatBytes(browserFile.Size)})");

                            var compressionResult = await jsModule.InvokeAsync<CompressionResult>(
                                "compressImage",
                                dataUrl,
                                Options.MaxFileSize,
                                Options.CompressionQualityLevels,
                                Options.MaxImageDimension
                            );

                            if (compressionResult.Success)
                            {
                                Console.WriteLine($"Compression successful: {FormatBytes(compressionResult.OriginalSize)} → {FormatBytes(compressionResult.CompressedSize)} (Quality: {compressionResult.Quality})");

                                // Use compressed data
                                dataUrl = compressionResult.Data;
                                var compressedBase64 = compressionResult.Data.Split(',')[1];
                                fileData = Convert.FromBase64String(compressedBase64);

                                ErrorMessages.Add(string.Format(Labels.ImageCompressed, browserFile.Name, FormatBytes(compressionResult.OriginalSize), FormatBytes(compressionResult.CompressedSize)));
                            }
                            else
                            {
                                ErrorMessages.Add(string.Format(Labels.ImageCompressionFailed, browserFile.Name, compressionResult.Message));
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error during compression: {ex.Message}");
                            ErrorMessages.Add(string.Format(Labels.ErrorDuringCompression, browserFile.Name, ex.Message));
                            continue;
                        }
                    }

                    fileItem = new FileUploadItem
                    {
                        FileName = browserFile.Name,
                        ContentType = browserFile.ContentType,
                        FileSize = fileData.Length,
                        FileData = fileData,
                        UploadDate = DateTime.Now
                    };

                    // Generate preview for images
                    if (fileItem.IsImage)
                    {
                        fileItem.ThumbnailUrl = dataUrl;
                    }
                }

                Files.Add(fileItem);
                await OnFileUploaded.InvokeAsync(fileItem);
            }
            catch (Exception ex)
            {
                ErrorMessages.Add($"Error uploading file '{browserFile.Name}': {ex.Message}");
            }
        }

        await FilesChanged.InvokeAsync(Files);
        StateHasChanged();
    }

    // Helper class for compression result
    private class CompressionResult
    {
        public bool Success { get; set; }
        public string Data { get; set; } = "";
        public long OriginalSize { get; set; }
        public long CompressedSize { get; set; }
        public double Quality { get; set; }
        public string Message { get; set; } = "";
    }

    private async Task HandleDrop(DragEventArgs e)
    {
        isDragging = false;
        
        if (!Options.AllowDragDrop)
            return;

        // Note: File drop handling through DragEventArgs is limited in Blazor
        // The actual files are handled through the InputFile component
        StateHasChanged();
    }

    private void HandleDragEnter(DragEventArgs e)
    {
        if (Options.AllowDragDrop)
        {
            isDragging = true;
        }
    }

    private void HandleDragLeave(DragEventArgs e)
    {
        isDragging = false;
    }

    public async Task PasteFromClipboard()
    {
        ErrorMessages.Clear();

        try
        {
            if (jsModule == null)
            {
                ErrorMessages.Add(Labels.ErrorJavaScriptModule);
                StateHasChanged();
                return;
            }

            if (dotNetHelper == null)
            {
                ErrorMessages.Add(Labels.ErrorComponentNotInitialized);
                StateHasChanged();
                return;
            }

            await jsModule.InvokeVoidAsync("readClipboard", dotNetHelper);
        }
        catch (JSException jsEx)
        {
            ErrorMessages.Add(string.Format(Labels.ErrorJavaScript, jsEx.Message));
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessages.Add(string.Format(Labels.ErrorReadingClipboard, ex.Message));
            StateHasChanged();
        }
    }

    [JSInvokable]
    public async Task HandleClipboardImage(string base64Data, string contentType)
    {
        Console.WriteLine($"HandleClipboardImage called - ContentType: {contentType}, Data length: {base64Data?.Length ?? 0}");

        try
        {
            if (string.IsNullOrEmpty(base64Data))
            {
                ErrorMessages.Add(Labels.ErrorNoImageData);
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Remove data URL prefix if present
            var base64String = base64Data;
            if (base64String.Contains(","))
            {
                base64String = base64String.Split(',')[1];
            }

            Console.WriteLine($"Base64 string length after split: {base64String.Length}");

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(base64String);
                Console.WriteLine($"Image bytes length: {imageBytes.Length}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting base64: {ex.Message}");
                ErrorMessages.Add(string.Format(Labels.ErrorDecodingImage, ex.Message));
                await InvokeAsync(StateHasChanged);
                return;
            }

            // ✨ NEW: Convert clipboard data to IBrowserFile
            // This allows clipboard images to be processed through the same pipeline as regular uploads
            var clipboardFile = new ClipboardBrowserFile(imageBytes, contentType);

            // ✨ NEW: Process through the standard file upload pipeline
            // This respects AutoUpload setting and uses Storage Service if configured
            if (Options.AutoUpload)
            {
                // Immediate upload (same as normal file selection)
                await ProcessFiles(new[] { clipboardFile });
            }
            else
            {
                // Stage for later upload (same as normal file selection)
                StageFiles(new[] { clipboardFile });
            }

            await InvokeAsync(StateHasChanged);
            Console.WriteLine("HandleClipboardImage completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in HandleClipboardImage: {ex}");
            ErrorMessages.Add(string.Format(Labels.ErrorProcessingClipboardImage, ex.Message));
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task DeleteFile(FileUploadItem file)
    {
        // If using storage service, delete from storage
        // But only if the file was actually stored in the service (not just in memory)
        if (StorageService != null)
        {
            // Check if file exists in storage before attempting to delete
            // Files from clipboard or not yet uploaded are only in memory
            try
            {
                bool existsInStorage = await StorageService.FileExistsAsync(file);

                if (existsInStorage)
                {
                    var deleted = await StorageService.DeleteFileAsync(file);
                    if (!deleted)
                    {
                        ErrorMessages.Add(string.Format(Labels.ErrorDeletingFile, file.FileName));
                    }
                }
                // If file doesn't exist in storage, it's only in memory - no need to delete from storage
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(string.Format(Labels.ErrorDeletingFileException, file.FileName, ex.Message));
                return;
            }
        }

        Files.Remove(file);
        await OnFileDeleted.InvokeAsync(file);
        await FilesChanged.InvokeAsync(Files);
        StateHasChanged();
    }

    private async Task DownloadFile(FileUploadItem file)
    {
        byte[]? fileData = null;

        // Get file data from storage service if available
        if (StorageService != null)
        {
            try
            {
                fileData = await StorageService.GetFileBytesAsync(file);
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(string.Format(Labels.ErrorDownloadingFile, file.FileName, ex.Message));
                StateHasChanged();
                return;
            }
        }
        else
        {
            fileData = file.FileData;
        }

        if (fileData != null)
        {
            var base64 = Convert.ToBase64String(fileData);
            var dataUrl = $"data:{file.ContentType};base64,{base64}";

            await JSRuntime.InvokeVoidAsync("eval", $@"
                const link = document.createElement('a');
                link.href = '{dataUrl}';
                link.download = '{file.FileName}';
                link.click();
            ");
        }

        await OnFileDownloaded.InvokeAsync(file);
    }

    private string GetFilePreviewUrl(FileUploadItem file)
    {
        // Return cached thumbnail if available
        if (!string.IsNullOrEmpty(file.ThumbnailUrl))
            return file.ThumbnailUrl;

        // For memory-based files (FileData contains actual image bytes)
        if (file.FileData != null && file.FileData.Length > 1024 && file.IsImage)
        {
            var base64 = Convert.ToBase64String(file.FileData);
            return $"data:{file.ContentType};base64,{base64}";
        }

        // For storage-based files or if thumbnail failed to load, return empty
        // (ThumbnailUrl should have been set during upload)
        return string.Empty;
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    public async ValueTask DisposeAsync()
    {
        if (jsModule != null)
        {
            try
            {
                await jsModule.DisposeAsync();
            }
            catch { }
        }

        dotNetHelper?.Dispose();
    }
}
