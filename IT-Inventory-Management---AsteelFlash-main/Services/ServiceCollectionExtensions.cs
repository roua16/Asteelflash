using ITStockM.Application.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace ITStockM.Services;

public static class ServiceCollectionExtensions
{
    [Obsolete("Use AddApplicationLayer from ITStockM.Application.DependencyInjection.")]
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services.AddApplicationLayer();
    }
}
