using BlazorFileManager.Test.Components;
using Microsoft.AspNetCore.SignalR;
using BlazorFileManager.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure SignalR to handle larger messages (for clipboard images)
builder.Services.Configure<HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
});

// Add File Storage Service
// Option 1: In-Memory (for testing)
// builder.Services.AddFileManagerInMemoryStorage();

// Option 2: Local File System (recommended for production)
builder.Services.AddFileManagerLocalStorage(
    basePath: Path.Combine(builder.Environment.ContentRootPath, "uploads"),
    maxFileSize: 50 * 1024 * 1024  // 50 MB
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
