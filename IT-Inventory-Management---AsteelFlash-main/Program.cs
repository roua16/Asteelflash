using ITStockM.Components;
using ITStockM.Application.DependencyInjection;
using ITStockM.Infrastructure.DependencyInjection;
using ITStockM.Presentation.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using ITStockM.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddApplicationLayer();
builder.Services.AddPresentationLayer(builder.Environment);

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

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.InitializeInfrastructureAsync();

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
