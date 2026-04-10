using ITStockM.Components;
using ITStockM.Services;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Implementation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Configure Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);

builder.Services.AddControllers();
builder.Services.AddRadzenComponents();

// HTTP Client
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<AppThemeService>();

// Services
// EmailOptions: bind from appsettings + allow env-var overrides at startup
builder.Services.Configure<EmailOptions>(opts =>
{
    builder.Configuration.GetSection(EmailOptions.Section).Bind(opts);
    opts.ApplyEnvironmentOverrides();
});
builder.Services.AddScoped<ITStockManagmentService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, ITStockM.Services.Implementation.NotificationService>();
builder.Services.AddScoped<IOperationNotificationService, OperationNotificationService>();
builder.Services.AddScoped<ITStockM.Services.Export.IExportService, ITStockM.Services.Export.ExportService>();

// Health check and SMTP helpers
builder.Services.AddSingleton<ITStockM.Services.Implementation.SmtpHealthChecker>();
builder.Services.AddSingleton<ITStockM.Services.Implementation.SmtpHealthHostedService>();
// Hosted service will check startup config before running
builder.Services.AddHostedService(provider => provider.GetRequiredService<ITStockM.Services.Implementation.SmtpHealthHostedService>());

// Database Context
builder.Services.AddDbContext<ITStockM.Data.ITStockManagmentContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ITStockManagmentConnection"));
});

// Application services and repositories
builder.Services.AddApplicationServices();

// Razor Pages and Blazor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable detailed Blazor circuit errors in Development for easier debugging
if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options => options.DetailedErrors = true);
}

// Authentication and Authorization
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "IT";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Strict;
    // Allows running behind HTTP locally/in Docker while still setting Secure cookies on HTTPS.
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddAuthorization();

// Background Services
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddHostedService<AssetHealthBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ITStockM.Data.ITStockManagmentContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        await ITStockM.Data.DatabaseInitializer.InitializeAsync(context, app.Environment, app.Configuration, logger);
    }
    catch (Exception ex)
    {
        var logger = services.GetService<ILoggerFactory>()?.CreateLogger("Startup");
        logger?.LogError(ex, "Startup initialization failed");
        Console.WriteLine("❌ Startup initialization failed: " + ex.Message);
    }
}

if (app.Environment.IsDevelopment())
{
    // Developer friendly: show detailed exception page and enable Blazor circuit detailed errors
    app.UseDeveloperExceptionPage();

    // Enable Swagger in Development
    app.UseSwagger();
    app.UseSwaggerUI();

    // In container/local HTTP scenarios, forcing HTTPS can break the app unless TLS is configured.
    app.UseHttpsRedirection();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
