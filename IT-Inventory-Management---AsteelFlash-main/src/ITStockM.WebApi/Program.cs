using ITStockM.Application.DependencyInjection;
using ITStockM.Components;
using ITStockM.Infrastructure.DependencyInjection;
using ITStockM.Infrastructure.Identity;
using ITStockM.WebApi.Middleware;
using ITStockM.WebApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Radzen;
using Serilog;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;

const string CorsPolicyName = "AppCorsPolicy";

Log.Logger = CreateBootstrapLogger();

try
{
    Log.Information("Starting ITStockM Application");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog(Log.Logger);

    ConfigureServices(builder.Services, builder.Configuration);

    var app = builder.Build();
    await ConfigurePipelineAsync(app);

    Log.Information("Application started successfully");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

static Serilog.Core.Logger CreateBootstrapLogger()
{
    return new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "ITStockM")
        .CreateLogger();
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:8080", "https://localhost:8081", "http://localhost:5080"];

    services.AddRazorComponents()
        .AddInteractiveServerComponents();
    services.AddCascadingAuthenticationState();

    services.AddRadzenComponents();
    services.AddScoped<DialogService>();
    services.AddScoped<NotificationService>();
    services.AddScoped<TooltipService>();

    services.AddAntiforgery();

    services.AddScoped<CustomAuthenticationStateProvider>();
    services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
    services.AddScoped<AppThemeService>();
    services.AddHttpContextAccessor();

    // Configure HttpClient for Blazor components
    services.AddScoped(sp =>
    {
        var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
        var request = httpContextAccessor.HttpContext?.Request;
        var baseAddress = request != null
            ? $"{request.Scheme}://{request.Host}"
            : "http://localhost:8080";
        
        var handler = new HttpClientHandler();
        
        // Enable cookie handling to preserve authentication
        if (httpContextAccessor.HttpContext?.Request.Cookies != null)
        {
            handler.UseCookies = true;
            // Create container and add cookies from current request
            var cookieContainer = new System.Net.CookieContainer();
            foreach (var cookie in httpContextAccessor.HttpContext.Request.Cookies)
            {
                try
                {
                    cookieContainer.Add(new Uri(baseAddress), new System.Net.Cookie(cookie.Key, cookie.Value));
                }
                catch
                {
                    // Skip invalid cookies
                }
            }
            handler.CookieContainer = cookieContainer;
        }
        
        var client = new HttpClient(handler) { BaseAddress = new Uri(baseAddress) };
        return client;
    });

    // Gemini Chat Services - Dependency Injection with Interface Abstraction
    services.AddScoped<GeminiChatStateService>();
    services.AddScoped<IGeminiService, GeminiChatService>();
    services.AddHttpClient<GeminiChatService>()
        .ConfigureHttpClient(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "ITStockM-AsteelFlash/1.0");
        });

    services.AddControllers(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    });
    services.AddCors(options =>
    {
        options.AddPolicy(CorsPolicyName, policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    services.AddInfrastructureLayer(configuration);
    services.AddApplicationLayer();

    services
        .AddIdentity<AppUser, IdentityRole<int>>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ITStockM.Data.ITStockManagmentContext>()
        .AddDefaultTokenProviders();

    services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.SlidingExpiration = true;
    });

    services.AddAuthorization();

    // One-time auth token store for Blazor → HTTP cookie exchange (tokens expire in 30 seconds)
    services.AddSingleton<ConcurrentDictionary<string, (UserSession Session, DateTime Expires)>>();

    services.AddHealthChecks()
        .AddDbContextCheck<ITStockM.Data.ITStockManagmentContext>(
            name: "Database",
            tags: ["database", "required"]);

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "IT Stock Management API",
            Version = "v1.0.0",
            Description = "Enterprise-level IT asset inventory management system",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Asteelflash"
            }
        });

        IncludeXmlCommentsIfExists(options, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
        IncludeXmlCommentsIfExists(options, $"{typeof(ApplicationLayerServiceCollectionExtensions).Assembly.GetName().Name}.xml");
    });
}

static void IncludeXmlCommentsIfExists(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options, string xmlFileName)
{
    var xmlFile = Path.Combine(AppContext.BaseDirectory, xmlFileName);
    if (File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }
}

static async Task ConfigurePipelineAsync(WebApplication app)
{
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.Use(SecurityHeadersMiddleware);

    await app.InitializeInfrastructureAsync();
    ConfigureEnvironmentMiddleware(app);

    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors(CorsPolicyName);
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapHealthChecks("/health");
    app.MapControllers();

    // Token-exchange endpoint: Blazor validates credentials, generates a short-lived token,
    // then JS fetches this endpoint so the browser receives a real Identity cookie.
    app.MapPost("/api/auth/sign-in/{token}", [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        async (HttpContext ctx, string token,
            ConcurrentDictionary<string, (UserSession Session, DateTime Expires)> store) =>
        {
            if (!store.TryRemove(token, out var entry) || entry.Expires < DateTime.UtcNow)
                return Results.Unauthorized();

            var session = entry.Session;
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, session.Id.ToString()),
                new Claim(ClaimTypes.Email, session.Email),
                new Claim(ClaimTypes.Name, session.FullName),
                new Claim(ClaimTypes.Role, session.Role),
            };
            var identity = new ClaimsIdentity(claims, "Identity.Application");
            var principal = new ClaimsPrincipal(identity);
            await ctx.SignInAsync("Identity.Application", principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true,
            });
            return Results.Ok();
        });

    app.MapPost("/api/auth/sign-out", [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync("Identity.Application");
            return Results.Ok();
        });
    app.MapGet("/index.html", () => Results.Redirect("/", permanent: false))
        .ExcludeFromDescription();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();
}

static void ConfigureEnvironmentMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "IT Stock Management API v1");
            c.RoutePrefix = "swagger";
        });
        app.UseHttpsRedirection();
        return;
    }

    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

static async Task SecurityHeadersMiddleware(HttpContext context, RequestDelegate next)
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    await next(context);
}
