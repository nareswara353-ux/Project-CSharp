using Azure.Identity;

namespace WebAPI.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddKeyVaultIfConfigured(this IConfigurationBuilder builder)
    {
        var tempConfig = builder.Build();
        var keyVaultUri = tempConfig["KeyVault:Uri"];

        if (string.IsNullOrWhiteSpace(keyVaultUri))
            return builder;

        var uri = new Uri(keyVaultUri);

        builder.AddAzureKeyVault(uri, new DefaultAzureCredential());

        return builder;
    }
}
