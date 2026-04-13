using ITStockM.Services;
using ITStockM.Services.AssetLifecycle;
using ITStockM.Services.AssignmentMateriels;
using ITStockM.Services.Assignments;
using ITStockM.Services.DeliveryOrderMateriels;
using ITStockM.Services.DeliveryOrders;
using ITStockM.Services.Employees;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Maintenance;
using ITStockM.Services.Materiels;
using ITStockM.Services.Offers;
using ITStockM.Services.Prediction;
using ITStockM.Services.Projects;
using ITStockM.Services.Requests;
using ITStockM.Services.Suppliers;
using Microsoft.Extensions.DependencyInjection;

namespace ITStockM.Application.DependencyInjection;

public static class ApplicationLayerServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
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

        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        return services;
    }
}