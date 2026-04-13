using ITStockM.Components;
using ITStockM.Services;
using ITStockM.Services.Export;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace ITStockM.Presentation.DependencyInjection;

public static class PresentationLayerServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationLayer(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);

        services.AddControllers();
        services.AddRadzenComponents();
        services.AddRazorPages();
        services.AddServerSideBlazor();

        services.AddScoped<AppThemeService>();
        services.AddScoped<ITStockManagmentService>();
        services.AddScoped<IExportService, ExportService>();

        services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        services.AddScoped<CustomAuthenticationStateProvider>();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "IT";
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/access-denied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            });

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        services.AddAuthorization();

        if (environment.IsDevelopment())
        {
            services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options => options.DetailedErrors = true);
        }

        return services;
    }
}