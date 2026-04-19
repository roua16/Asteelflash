using FluentValidation;
using ITStockM.Application.Common.Behaviors;
using ITStockM.Application.Common.Mapping;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ITStockM.Application.DependencyInjection;

public static class ApplicationLayerServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        services.AddMediatR(typeof(ApplicationLayerServiceCollectionExtensions).Assembly);
        services.AddValidatorsFromAssembly(typeof(ApplicationLayerServiceCollectionExtensions).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}