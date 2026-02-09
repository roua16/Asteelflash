using Microsoft.Extensions.DependencyInjection;
using ITStockM.Repositories;

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

        // Keep existing services already registered in Program.cs, or register here if you prefer centralization
        return services;
    }
}
