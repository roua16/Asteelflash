using ITStockM.Components;
using ITStockM.Application.DependencyInjection;
using ITStockM.Infrastructure.DependencyInjection;
using ITStockM.Presentation.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddApplicationLayer();
builder.Services.AddPresentationLayer(builder.Environment);

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
