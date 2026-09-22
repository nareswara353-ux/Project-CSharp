using Application.Common;
using Infrastructure.Email;
using Infrastructure.Jobs;
using Infrastructure.Observability;
using Microsoft.Extensions.Options;

namespace WebAPI.Extensions;

public static class OptionsValidationExtensions
{
    public static IServiceCollection AddValidatedOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.Secret) && settings.Secret.Length >= 32,
                "Jwt:Secret must be at least 32 characters.")
            .Validate(
                settings => settings.ExpirationMinutes > 0,
                "Jwt:ExpirationMinutes must be greater than zero.")
            .ValidateOnStart();

        services
            .AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                settings => !settings.Enabled || !string.IsNullOrWhiteSpace(settings.Host),
                "Email:Host is required when Email is enabled.")
            .ValidateOnStart();

        services
            .AddOptions<ObservabilitySettings>()
            .Bind(configuration.GetSection(ObservabilitySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<HangfireSettings>()
            .Bind(configuration.GetSection(HangfireSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
