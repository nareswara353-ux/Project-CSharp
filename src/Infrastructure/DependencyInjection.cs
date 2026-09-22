using Application.Common;
using Application.Common.Behaviors;
using Domain.Repositories;
using Hangfire;
using Infrastructure.Caching;
using Infrastructure.Common;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Email;
using Infrastructure.Events;
using Infrastructure.Jobs;
using Infrastructure.Pricing;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null)
                    .CommandTimeout(60)));

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IOrderRepository, EfOrderRepository>();
        services.AddScoped<IUserRepository, EfUserRepository>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IEmailTemplateRenderer, SimpleEmailTemplateRenderer>();

        services.AddScoped<OrderCleanupJob>();
        services.AddScoped<DailyReportJob>();

        var hangfireSettings = configuration
            .GetSection(HangfireSettings.SectionName)
            .Get<HangfireSettings>() ?? new HangfireSettings();

        if (hangfireSettings.Enabled && !string.IsNullOrWhiteSpace(hangfireSettings.ConnectionString))
        {
            services.Configure<HangfireSettings>(configuration.GetSection(HangfireSettings.SectionName));

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(hangfireSettings.ConnectionString));

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = hangfireSettings.WorkerCount;
                options.Queues = new[] { "default" };
            });

            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
        }
        else
        {
            services.AddSingleton<IBackgroundJobService, InMemoryBackgroundJobService>();
            services.AddHostedService<BackgroundJobScheduler>();
        }

        services.AddCaching(configuration);
        services.AddSingleton<IDiscountService, StaticDiscountService>();

        return services;
    }
}
