# BlazorFileManager - Beispiele

## Integration in ein bestehendes Blazor-Projekt

### 1. Package Reference hinzufügen

```xml
<ItemGroup>
  <ProjectReference Include="..\BlazorFileManager\BlazorFileManager.csproj" />
</ItemGroup>
```

### 2. JavaScript-Datei einbinden

In `wwwroot/index.html` (Blazor WebAssembly) oder `Pages/_Layout.cshtml` (Blazor Server):

```html
<script src="_content/BlazorFileManager/fileUploadManager.js"></script>
```

### 3. Namespace importieren

In `_Imports.razor`:

```razor
@using BlazorFileManager.Components
```

## Anwendungsbeispiele

### Beispiel 1: Anhänge für ein Ticket-System

```razor
@page "/tickets/edit/{TicketId:int}"
@using BlazorFileManager.Components

<h3>Ticket bearbeiten</h3>

<EditForm Model="ticket" OnValidSubmit="SaveTicket">
    <div class="mb-3">
        <label>Titel</label>
        <InputText @bind-Value="ticket.Title" class="form-control" />
    </div>

    <div class="mb-3">
        <label>Beschreibung</label>
        <InputTextArea @bind-Value="ticket.Description" class="form-control" />
    </div>

    <div class="mb-3">
        <label>Anhänge</label>
        <FileUploadManager 
            @bind-Files="ticketAttachments"
            Title="Ticket-Anhänge"
            Options="attachmentOptions"
            OnFileUploaded="OnAttachmentAdded"
            OnFileDeleted="OnAttachmentRemoved" />
    </div>

    <button type="submit" class="btn btn-primary">Speichern</button>
</EditForm>

@code {
    [Parameter]
    public int TicketId { get; set; }

    [Inject]
    private HttpClient Http { get; set; }

    [Inject]
    private NavigationManager Navigation { get; set; }

    private Ticket ticket = new();
    private List<FileUploadItem> ticketAttachments = new();

    private FileUploadOptions attachmentOptions = new()
    {
        MaxFileCount = 10,
        MaxFileSize = 10 * 1024 * 1024, // 10MB
        AllowedFileTypes = new List<string> 
        { 
            "image/jpeg", "image/png", "image/gif",
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        },
        UploadPath = $"tickets/{TicketId}/attachments"
    };

    protected override async Task OnInitializedAsync()
    {
        // Load ticket and attachments
        ticket = await Http.GetFromJsonAsync<Ticket>($"api/tickets/{TicketId}");
        ticketAttachments = await Http.GetFromJsonAsync<List<FileUploadItem>>($"api/tickets/{TicketId}/attachments") ?? new();
    }

    private async Task SaveTicket()
    {
        await Http.PutAsJsonAsync($"api/tickets/{TicketId}", ticket);
        Navigation.NavigateTo("/tickets");
    }

    private async Task OnAttachmentAdded(FileUploadItem file)
    {
        // Upload to server
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(file.FileData!);
        content.Add(fileContent, "file", file.FileName);

        await Http.PostAsync($"api/tickets/{TicketId}/attachments", content);
    }

    private async Task OnAttachmentRemoved(FileUploadItem file)
    {
        await Http.DeleteAsync($"api/tickets/{TicketId}/attachments/{file.Id}");
    }

    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
```

### Beispiel 2: Produkt-Bildergalerie

```razor
@page "/products/edit/{ProductId:int}"
@using BlazorFileManager.Components

<h3>Produktbilder verwalten</h3>

<FileUploadManager 
    @bind-Files="productImages"
    Title="Produktbilder"
    Options="imageOptions"
    CustomFileProcessor="ProcessProductImage">
    
    <FileItemTemplate Context="image">
        <div class="product-image-card">
            <img src="@GetImageUrl(image)" alt="@image.FileName" />
            <div class="image-info">
                <strong>@image.FileName</strong>
                <div class="image-actions">
                    <button @onclick="() => SetAsMainImage(image)" class="btn btn-sm btn-info">
                        Als Hauptbild
                    </button>
                    @if (Options.AllowDelete)
                    {
                        <button @onclick="() => DeleteImage(image)" class="btn btn-sm btn-danger">
                            Löschen
                        </button>
                    }
                </div>
            </div>
        </div>
    </FileItemTemplate>
</FileUploadManager>

<style>
    .product-image-card {
        display: grid;
        grid-template-columns: 150px 1fr;
        gap: 1rem;
        padding: 1rem;
        border: 1px solid #ddd;
        border-radius: 8px;
        margin-bottom: 0.5rem;
    }

    .product-image-card img {
        width: 150px;
        height: 150px;
        object-fit: cover;
        border-radius: 4px;
    }

    .image-info {
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }

    .image-actions {
        display: flex;
        gap: 0.5rem;
    }
</style>

@code {
    [Parameter]
    public int ProductId { get; set; }

    [Inject]
    private HttpClient Http { get; set; }

    private List<FileUploadItem> productImages = new();

    private FileUploadOptions imageOptions = new()
    {
        MaxFileCount = 8,
        MaxFileSize = 5 * 1024 * 1024, // 5MB
        AllowedFileTypes = new List<string> { "image/jpeg", "image/png", "image/webp" },
        AcceptAttribute = "image/*",
        AllowDelete = true,
        ShowPreview = true
    };

    protected override async Task OnInitializedAsync()
    {
        productImages = await Http.GetFromJsonAsync<List<FileUploadItem>>($"api/products/{ProductId}/images") ?? new();
    }

    private async Task<FileUploadItem> ProcessProductImage(IBrowserFile browserFile)
    {
        // Resize and optimize image on server
        using var content = new MultipartFormDataContent();
        var stream = browserFile.OpenReadStream(5 * 1024 * 1024);
        var streamContent = new StreamContent(stream);
        content.Add(streamContent, "image", browserFile.Name);

        var response = await Http.PostAsync($"api/products/{ProductId}/images", content);
        return await response.Content.ReadFromJsonAsync<FileUploadItem>();
    }

    private async Task SetAsMainImage(FileUploadItem image)
    {
        await Http.PostAsync($"api/products/{ProductId}/main-image/{image.Id}", null);
    }

    private async Task DeleteImage(FileUploadItem image)
    {
        await Http.DeleteAsync($"api/products/{ProductId}/images/{image.Id}");
        productImages.Remove(image);
    }

    private string GetImageUrl(FileUploadItem image)
    {
        return !string.IsNullOrEmpty(image.ThumbnailUrl) 
            ? image.ThumbnailUrl 
            : $"api/products/{ProductId}/images/{image.Id}";
    }
}
```

### Beispiel 3: Dokument-Management mit Kategorien

```razor
@page "/documents"
@using BlazorFileManager.Components

<h3>Dokumenten-Verwaltung</h3>

<div class="row">
    <div class="col-md-4">
        <div class="card">
            <div class="card-header">Verträge</div>
            <div class="card-body">
                <FileUploadManager 
                    @bind-Files="contracts"
                    Title="Verträge"
                    Options="contractOptions"
                    CustomFileProcessor="@(file => ProcessDocument(file, DocumentCategory.Contract))" />
            </div>
        </div>
    </div>

    <div class="col-md-4">
        <div class="card">
            <div class="card-header">Rechnungen</div>
            <div class="card-body">
                <FileUploadManager 
                    @bind-Files="invoices"
                    Title="Rechnungen"
                    Options="invoiceOptions"
                    CustomFileProcessor="@(file => ProcessDocument(file, DocumentCategory.Invoice))" />
            </div>
        </div>
    </div>

    <div class="col-md-4">
        <div class="card">
            <div class="card-header">Sonstiges</div>
            <div class="card-body">
                <FileUploadManager 
                    @bind-Files="miscDocuments"
                    Title="Sonstige Dokumente"
                    Options="miscOptions"
                    CustomFileProcessor="@(file => ProcessDocument(file, DocumentCategory.Miscellaneous))" />
            </div>
        </div>
    </div>
</div>

@code {
    [Inject]
    private HttpClient Http { get; set; }

    private List<FileUploadItem> contracts = new();
    private List<FileUploadItem> invoices = new();
    private List<FileUploadItem> miscDocuments = new();

    private FileUploadOptions contractOptions = new()
    {
        MaxFileCount = 0, // Unlimited
        MaxFileSize = 20 * 1024 * 1024, // 20MB
        AllowedFileTypes = new List<string> { "application/pdf" },
        AcceptAttribute = ".pdf",
        UploadPath = "documents/contracts"
    };

    private FileUploadOptions invoiceOptions = new()
    {
        MaxFileCount = 0,
        MaxFileSize = 10 * 1024 * 1024,
        AllowedFileTypes = new List<string> { "application/pdf", "image/jpeg", "image/png" },
        AcceptAttribute = ".pdf,image/*",
        UploadPath = "documents/invoices"
    };

    private FileUploadOptions miscOptions = new()
    {
        MaxFileCount = 50,
        MaxFileSize = 15 * 1024 * 1024,
        UploadPath = "documents/misc"
    };

    protected override async Task OnInitializedAsync()
    {
        var allDocs = await Http.GetFromJsonAsync<List<Document>>("api/documents") ?? new();
        
        contracts = allDocs.Where(d => d.Category == DocumentCategory.Contract)
            .Select(d => d.ToFileUploadItem()).ToList();
        
        invoices = allDocs.Where(d => d.Category == DocumentCategory.Invoice)
            .Select(d => d.ToFileUploadItem()).ToList();
        
        miscDocuments = allDocs.Where(d => d.Category == DocumentCategory.Miscellaneous)
            .Select(d => d.ToFileUploadItem()).ToList();
    }

    private async Task<FileUploadItem> ProcessDocument(IBrowserFile browserFile, DocumentCategory category)
    {
        using var content = new MultipartFormDataContent();
        var stream = browserFile.OpenReadStream(20 * 1024 * 1024);
        var streamContent = new StreamContent(stream);
        content.Add(streamContent, "file", browserFile.Name);
        content.Add(new StringContent(category.ToString()), "category");

        var response = await Http.PostAsync("api/documents", content);
        var document = await response.Content.ReadFromJsonAsync<Document>();
        
        return document!.ToFileUploadItem();
    }

    public enum DocumentCategory
    {
        Contract,
        Invoice,
        Miscellaneous
    }

    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public long FileSize { get; set; }
        public string FilePath { get; set; } = "";
        public DocumentCategory Category { get; set; }
        public DateTime UploadDate { get; set; }

        public FileUploadItem ToFileUploadItem()
        {
            return new FileUploadItem
            {
                Id = Id.ToString(),
                FileName = FileName,
                ContentType = ContentType,
                FileSize = FileSize,
                FilePath = FilePath,
                UploadDate = UploadDate
            };
        }
    }
}
```

### Beispiel 4: Chat mit Dateianhängen

```razor
@page "/chat/{ConversationId:int}"
@using BlazorFileManager.Components

<div class="chat-container">
    <div class="messages">
        @foreach (var message in messages)
        {
            <div class="message @(message.IsOwn ? "own" : "other")">
                <div class="message-content">@message.Text</div>
                @if (message.Attachments.Any())
                {
                    <div class="message-attachments">
                        @foreach (var attachment in message.Attachments)
                        {
                            @if (attachment.IsImage)
                            {
                                <img src="@attachment.ThumbnailUrl" alt="@attachment.FileName" class="attachment-image" />
                            }
                            else
                            {
                                <a href="#" @onclick="() => DownloadAttachment(attachment)">
                                    📎 @attachment.FileName (@attachment.FormattedFileSize)
                                </a>
                            }
                        }
                    </div>
                }
            </div>
        }
    </div>

    <div class="message-input">
        <input type="text" @bind="newMessageText" @onkeypress="HandleKeyPress" placeholder="Nachricht eingeben..." />
        
        <FileUploadManager 
            @bind-Files="pendingAttachments"
            Options="chatAttachmentOptions">
            <HeaderTemplate>
                <div style="display: none;"></div>
            </HeaderTemplate>
            <DropZoneTemplate>
                <button type="button" class="btn btn-secondary">📎 Anhang</button>
            </DropZoneTemplate>
            <FileItemTemplate Context="file">
                <span class="badge bg-info">@file.FileName</span>
            </FileItemTemplate>
        </FileUploadManager>

        <button @onclick="SendMessage" class="btn btn-primary">Senden</button>
    </div>
</div>

@code {
    [Parameter]
    public int ConversationId { get; set; }

    [Inject]
    private HttpClient Http { get; set; }

    private List<ChatMessage> messages = new();
    private string newMessageText = "";
    private List<FileUploadItem> pendingAttachments = new();

    private FileUploadOptions chatAttachmentOptions = new()
    {
        MaxFileCount = 5,
        MaxFileSize = 10 * 1024 * 1024,
        AllowPasteFromClipboard = true,
        ShowPreview = false
    };

    protected override async Task OnInitializedAsync()
    {
        messages = await Http.GetFromJsonAsync<List<ChatMessage>>($"api/chat/{ConversationId}/messages") ?? new();
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(newMessageText) && !pendingAttachments.Any())
            return;

        var message = new ChatMessage
        {
            Text = newMessageText,
            Attachments = new List<FileUploadItem>(pendingAttachments),
            IsOwn = true,
            Timestamp = DateTime.Now
        };

        messages.Add(message);

        // Send to server
        await Http.PostAsJsonAsync($"api/chat/{ConversationId}/messages", message);

        newMessageText = "";
        pendingAttachments.Clear();
    }

    private void HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            SendMessage();
        }
    }

    private void DownloadAttachment(FileUploadItem attachment)
    {
        // Download logic
    }

    public class ChatMessage
    {
        public string Text { get; set; } = "";
        public List<FileUploadItem> Attachments { get; set; } = new();
        public bool IsOwn { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
```

## Best Practices

### 1. Server-seitiges Speichern

Verwenden Sie `CustomFileProcessor` um Dateien auf dem Server zu speichern statt im Browser-Speicher:

```csharp
private async Task<FileUploadItem> SaveToServer(IBrowserFile browserFile)
{
    using var content = new MultipartFormDataContent();
    var stream = browserFile.OpenReadStream(maxFileSize);
    var streamContent = new StreamContent(stream);
    content.Add(streamContent, "file", browserFile.Name);

    var response = await Http.PostAsync("api/files/upload", content);
    var uploadedFile = await response.Content.ReadFromJsonAsync<FileUploadItem>();
    
    return uploadedFile;
}
```

### 2. Validierung

Nutzen Sie die `AllowedFileTypes` für clientseitige Validierung, validieren Sie aber immer auch serverseitig:

```csharp
// Server-Side (API Controller)
[HttpPost("upload")]
public async Task<IActionResult> Upload(IFormFile file)
{
    var allowedTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
    
    if (!allowedTypes.Contains(file.ContentType))
    {
        return BadRequest("File type not allowed");
    }
    
    if (file.Length > 10 * 1024 * 1024)
    {
        return BadRequest("File too large");
    }
    
    // Save file...
    return Ok();
}
```

### 3. Performance

Für große Dateien, verwenden Sie Streaming statt das Laden der kompletten Datei in den Speicher:

```csharp
private async Task<FileUploadItem> StreamToServer(IBrowserFile browserFile)
{
    using var stream = browserFile.OpenReadStream(maxAllowedSize: 100 * 1024 * 1024);
    
    var response = await Http.PostAsync(
        "api/files/upload/stream",
        new StreamContent(stream)
    );
    
    return await response.Content.ReadFromJsonAsync<FileUploadItem>();
}
```

### 4. Fehlerbehandlung

Implementieren Sie umfassende Fehlerbehandlung:

```csharp
private async Task<FileUploadItem> SafeFileProcessor(IBrowserFile browserFile)
{
    try
    {
        // Process file...
        return await ProcessFile(browserFile);
    }
    catch (IOException ex)
    {
        Logger.LogError(ex, "IO Error processing file {FileName}", browserFile.Name);
        throw new Exception($"Fehler beim Lesen der Datei: {ex.Message}");
    }
    catch (HttpRequestException ex)
    {
        Logger.LogError(ex, "Network error uploading file {FileName}", browserFile.Name);
        throw new Exception($"Netzwerkfehler beim Upload: {ex.Message}");
    }
}
```
