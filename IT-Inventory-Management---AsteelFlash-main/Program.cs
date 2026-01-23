using ITStockM.Components;
using ITStockM.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Radzen;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);
builder.Services.AddControllers();
builder.Services.AddRadzenComponents();

builder.Services.AddHttpClient();
builder.Services.AddScoped<ITStockManagmentService>();

builder.Services.AddDbContext<ITStockM.Data.ITStockManagmentContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ITStockManagmentConnection"));
});


builder.Services.AddRazorPages(); 
builder.Services.AddServerSideBlazor(); 

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
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
builder.Services.AddAuthorization();
builder.Services.AddSingleton<EmailService>(new EmailService(
    smtpServer: "smtp.office365.com", 
    smtpPort: 587,
    fromEmail: "griramed@hotmail.com",
    password: "ylpysozvumgdtfgb" 
));

builder.Services.AddHostedService<EmailBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ITStockM.Data.ITStockManagmentContext>();
        context.Database.Migrate(); 
        Console.WriteLine("✔️ Migration exécutée avec succès.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Erreur de migration : " + ex.Message);
    }
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();   
app.UseAntiforgery();    
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();