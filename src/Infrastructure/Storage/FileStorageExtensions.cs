using Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Storage;

public static class FileStorageExtensions
{
    public static IServiceCollection AddFileStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(FileStorageSettings.SectionName)
            .Get<FileStorageSettings>() ?? new FileStorageSettings();

        services.Configure<FileStorageSettings>(
            configuration.GetSection(FileStorageSettings.SectionName));

        switch (settings.Provider)
        {
            case FileStorageProviders.Local:
                services.AddScoped<IFileStorage, LocalFileStorage>();
                break;

            case FileStorageProviders.AzureBlob:
                services.AddSingleton<IFileStorage, AzureBlobFileStorage>();
                break;

            case FileStorageProviders.None:
            default:
                services.AddScoped<IFileStorage, NoOpFileStorage>();
                break;
        }

        return services;
    }
}
