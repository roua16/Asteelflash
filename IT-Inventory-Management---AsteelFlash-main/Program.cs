using ITStockM.Application.DependencyInjection;
using ITStockM.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using ITStockM.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
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
app.MapControllers();
app.Run();
