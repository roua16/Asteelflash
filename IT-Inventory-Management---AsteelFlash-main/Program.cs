using ITStockM.Application.DependencyInjection;
using ITStockM.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using ITStockM.Infrastructure.Identity;
using ITStockM.WebApi.Middleware;
using Serilog;

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ITStockM")
    .CreateLogger();

try
{
    Log.Information("Starting ITStockM Application");
    
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog(Log.Logger);

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    builder.Services.AddInfrastructureLayer(builder.Configuration);
    builder.Services.AddApplicationLayer();

    // Configure ASP.NET Core Identity
    builder.Services
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

    // Add Authorization
    builder.Services.AddAuthorization();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ITStockM.Data.ITStockManagmentContext>(
            name: "Database",
            tags: ["database", "required"]);

    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
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

        // Include XML comments in swagger if available
        var xmlFile = Path.Combine(AppContext.BaseDirectory, "ITStockM.xml");
        if (File.Exists(xmlFile))
        {
            options.IncludeXmlComments(xmlFile);
        }
    });

    var app = builder.Build();

    // Add global exception handling middleware (must be before other middleware)
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Add security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        await next();
    });

    await app.InitializeInfrastructureAsync();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "IT Stock Management API v1");
            c.RoutePrefix = string.Empty;
        });
        app.UseHttpsRedirection();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.UseStaticFiles();
    app.UseCors("AllowAll");
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    
    // Map health checks
    app.MapHealthChecks("/health");
    
    // Map controllers
    app.MapControllers();

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
