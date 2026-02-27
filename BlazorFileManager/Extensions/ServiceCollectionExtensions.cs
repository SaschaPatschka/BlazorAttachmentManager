using BlazorFileManager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorFileManager.Extensions;

/// <summary>
/// Extension methods for registering FileManager services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds file storage services to the dependency injection container.
    /// Uses in-memory storage by default.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFileManagerInMemoryStorage(this IServiceCollection services)
    {
        services.AddScoped<IFileStorageService, InMemoryFileStorageService>();
        return services;
    }

    /// <summary>
    /// Adds file storage services to the dependency injection container.
    /// Uses local file system storage.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="basePath">Base directory path where files will be stored. If null, uses "uploads" folder.</param>
    /// <param name="maxFileSize">Maximum file size in bytes (default: 10 MB)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFileManagerLocalStorage(
        this IServiceCollection services,
        string? basePath = null,
        long maxFileSize = 10 * 1024 * 1024)
    {
        services.AddScoped<IFileStorageService>(sp =>
        {
            var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<LocalFileStorageService>>();
            return new LocalFileStorageService(basePath, logger, maxFileSize);
        });
        return services;
    }

    /// <summary>
    /// Adds a custom file storage service implementation to the dependency injection container.
    /// </summary>
    /// <typeparam name="TImplementation">The implementation type that implements IFileStorageService</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFileManagerCustomStorage<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IFileStorageService
    {
        services.AddScoped<IFileStorageService, TImplementation>();
        return services;
    }
}
