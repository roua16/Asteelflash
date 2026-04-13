using ITStockM.Data;
using ITStockM.Repositories;
using ITStockM.Services;
using ITStockM.Services.Implementation;
using ITStockM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ITStockM.Infrastructure.DependencyInjection;

public static class InfrastructureLayerServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.AddMemoryCache();

        services.Configure<EmailOptions>(options =>
        {
            configuration.GetSection(EmailOptions.Section).Bind(options);
            options.ApplyEnvironmentOverrides();
        });

        services.AddDbContext<ITStockManagmentContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("ITStockManagmentConnection"));
        });

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IMaterielRepository, MaterielRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddScoped<IDeliveryOrderRepository, DeliveryOrderRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOperationNotificationService, OperationNotificationService>();
        services.AddScoped<IWarrantyAlertService, WarrantyAlertService>();

        services.AddSingleton<SmtpHealthChecker>();
        services.AddSingleton<SmtpHealthHostedService>();
        services.AddHostedService(provider => provider.GetRequiredService<SmtpHealthHostedService>());

        services.AddHostedService<EmailBackgroundService>();
        services.AddHostedService<AssetHealthBackgroundService>();

        return services;
    }
}