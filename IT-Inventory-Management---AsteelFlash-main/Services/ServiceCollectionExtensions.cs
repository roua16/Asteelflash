using Microsoft.Extensions.DependencyInjection;
using ITStockM.Repositories;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Implementation;
using ITStockM.Services.AssetLifecycle;
using ITStockM.Services.Maintenance;
using ITStockM.Services.Prediction;

namespace ITStockM.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IMaterielRepository, MaterielRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddScoped<IDeliveryOrderRepository, DeliveryOrderRepository>();

        // Domain services
        services.AddScoped<ITStockM.Services.Assignments.IAssignmentService, ITStockM.Services.Assignments.AssignmentService>();
        services.AddScoped<ITStockM.Services.Materiels.IMaterielService, ITStockM.Services.Materiels.MaterielService>();
        services.AddScoped<ITStockM.Services.Requests.IRequestService, ITStockM.Services.Requests.RequestService>();
        services.AddScoped<ITStockM.Services.DeliveryOrders.IDeliveryOrderService, ITStockM.Services.DeliveryOrders.DeliveryOrderService>();
        services.AddScoped<ITStockM.Services.Employees.IEmployeeService, ITStockM.Services.Employees.EmployeeService>();
        services.AddScoped<ITStockM.Services.Projects.IProjectService, ITStockM.Services.Projects.ProjectService>();
        services.AddScoped<ITStockM.Services.Suppliers.ISupplierService, ITStockM.Services.Suppliers.SupplierService>();
        services.AddScoped<ITStockM.Services.Offers.IOfferService, ITStockM.Services.Offers.OfferService>();
        services.AddScoped<ITStockM.Services.AssignmentMateriels.IAssignmentMaterielService, ITStockM.Services.AssignmentMateriels.AssignmentMaterielService>();
        services.AddScoped<ITStockM.Services.DeliveryOrderMateriels.IDeliveryOrderMaterielService, ITStockM.Services.DeliveryOrderMateriels.DeliveryOrderMaterielService>();

        // Asset Lifecycle Module
        services.AddScoped<IAssetLifecycleService, AssetLifecycleService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IPredictionService, PredictionService>();
        services.AddScoped<IWarrantyAlertService, WarrantyAlertService>();
        // IEmailTemplateService is singleton — stateless HTML builder
        services.AddSingleton<IEmailTemplateService, EmailTemplateService>();

        return services;
    }
}
