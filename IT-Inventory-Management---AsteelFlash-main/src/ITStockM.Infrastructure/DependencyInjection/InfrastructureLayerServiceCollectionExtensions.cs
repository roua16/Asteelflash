using ITStockM.Application.Common.Interfaces;
using ITStockM.Data;
using ITStockM.Repositories;
using ITStockM.Services;
using ITStockM.Services.AssetLifecycle;
using ITStockM.Services.AssignmentMateriels;
using ITStockM.Services.Assignments;
using ITStockM.Services.DeliveryOrderMateriels;
using ITStockM.Services.DeliveryOrders;
using ITStockM.Services.Employees;
using ITStockM.Services.Implementation;
using ITStockM.Services.Interfaces;
using ITStockM.Services.LanSweeper;
using ITStockM.Services.Maintenance;
using ITStockM.Services.Materiels;
using ITStockM.Services.Offers;
using ITStockM.Services.Prediction;
using ITStockM.Services.Projects;
using ITStockM.Services.Requests;
using ITStockM.Services.Suppliers;
using ITStockM.Infrastructure.Services;
using ITStockM.Infrastructure.Services.ActiveDirectory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ITStockM.Infrastructure.DependencyInjection;

public static class InfrastructureLayerServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();

        services.Configure<EmailOptions>(options =>
        {
            configuration.GetSection(EmailOptions.Section).Bind(options);
            options.ApplyEnvironmentOverrides();
        });

        services.AddDbContext<ITStockManagmentContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("ITStockManagmentConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'ITStockManagmentConnection' is not configured.");
            }

            if (IsSqlServerConnection(connectionString))
            {
                options.UseSqlServer(connectionString);
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ITStockManagmentContext>());
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEventPublisher, EventPublisher>();

        // Active Directory / LDAP – configuration-driven; disabled by default in dev
        services.Configure<ActiveDirectoryOptions>(
            configuration.GetSection(ActiveDirectoryOptions.Section));
        services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IMaterielRepository, MaterielRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddScoped<IDeliveryOrderRepository, DeliveryOrderRepository>();

        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IMaterielService, MaterielService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IDeliveryOrderService, DeliveryOrderService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IOfferService, OfferService>();
        services.AddScoped<IAssignmentMaterielService, AssignmentMaterielService>();
        services.AddScoped<IDeliveryOrderMaterielService, DeliveryOrderMaterielService>();

        services.AddScoped<IAssetLifecycleService, AssetLifecycleService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IPredictionService, PredictionService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOperationNotificationService, OperationNotificationService>();
        services.AddScoped<IWarrantyAlertService, WarrantyAlertService>();
        services.AddScoped<ILanSweeperService, LanSweeperService>();

        services.AddSingleton<SmtpHealthChecker>();
        services.AddSingleton<SmtpHealthHostedService>();
        services.AddHostedService(provider => provider.GetRequiredService<SmtpHealthHostedService>());

        services.AddHostedService<EmailBackgroundService>();
        services.AddHostedService<AssetHealthBackgroundService>();

        return services;
    }

    private static bool IsSqlServerConnection(string connectionString)
    {
        return connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("Trusted_Connection=", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("MultipleActiveResultSets=", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("User Id=", StringComparison.OrdinalIgnoreCase);
    }
}
