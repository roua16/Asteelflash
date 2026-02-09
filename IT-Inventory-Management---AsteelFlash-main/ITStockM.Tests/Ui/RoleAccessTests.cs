using Microsoft.Playwright;
using Xunit;

namespace ITStockM.Tests.Ui;

public class RoleAccessTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture fixture;

    private static string BaseUrl => Environment.GetEnvironmentVariable("UI_BASE_URL") ?? "http://localhost:8080";

    public RoleAccessTests(PlaywrightFixture fixture)
    {
        this.fixture = fixture;
    }

    private async Task<IPage> LoginAsync(IBrowserContext context, string email, string password)
    {
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{BaseUrl}/login");
        await page.FillAsync("#email", email);
        await page.FillAsync("#password", password);
        await page.ClickAsync("button.login-button");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        try
        {
            await page.Locator("a[href='/logout']").WaitForAsync(new() { Timeout = 10000 });
        }
        catch
        {
            var errorText = await page.Locator(".rz-alert").TextContentAsync();
            throw new Exception($"Login failed or UI not ready. Error: {errorText ?? "(none)"}");
        }

        await page.WaitForSelectorAsync("text=IT Inventory");
        return page;
    }

    [UiFact]
    [Trait("Category", "UI")]
    public async Task Admin_Sees_Administration_Menu()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await LoginAsync(context, "admin@asteelflash.com", "admin123");

        Assert.True(await page.GetByText("Dashboard", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("Administration", new() { Exact = true }).IsVisibleAsync());

        await page.GotoAsync($"{BaseUrl}/materiels-super-admin");
        Assert.False(await page.GetByText("Access Denied").IsVisibleAsync());
    }

    [UiFact]
    [Trait("Category", "UI")]
    public async Task Pdr_Sees_Pdr_Materials_And_Suppliers()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await LoginAsync(context, "john.smith@asteelflash.com", "password123");

        Assert.True(await page.GetByText("Materials View (PDR)", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("Suppliers", new() { Exact = true }).IsVisibleAsync());
        Assert.False(await page.GetByText("Administration", new() { Exact = true }).IsVisibleAsync());

        await page.GotoAsync($"{BaseUrl}/materials-view-interface-pdr");
        Assert.False(await page.GetByText("Access Denied").IsVisibleAsync());
    }

    [UiFact]
    [Trait("Category", "UI")]
    public async Task Purchasing_Sees_Purchasing_Pages()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await LoginAsync(context, "sarah.johnson@asteelflash.com", "password123");

        Assert.True(await page.GetByText("Purchase Requests", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("Archived Requests", new() { Exact = true }).IsVisibleAsync());
        Assert.False(await page.GetByText("Administration", new() { Exact = true }).IsVisibleAsync());

        await page.GotoAsync($"{BaseUrl}/purchase-page");
        Assert.False(await page.GetByText("Access Denied").IsVisibleAsync());
    }

    [UiFact]
    [Trait("Category", "UI")]
    public async Task It_Sees_Assignments_And_Infrastructure()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await LoginAsync(context, "mike.davis@asteelflash.com", "password123");

        Assert.True(await page.GetByText("Materials Assignments", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("Infrastructure", new() { Exact = true }).IsVisibleAsync());
        Assert.False(await page.GetByText("Administration", new() { Exact = true }).IsVisibleAsync());

        await page.GotoAsync($"{BaseUrl}/assignments-interface");
        Assert.False(await page.GetByText("Access Denied").IsVisibleAsync());
    }

    [UiFact]
    [Trait("Category", "UI")]
    public async Task Infrastructure_Sees_Infrastructure_Page()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await LoginAsync(context, "alice.brown@asteelflash.com", "password123");

        Assert.True(await page.GetByText("Infrastructure", new() { Exact = true }).IsVisibleAsync());
        Assert.False(await page.GetByText("Administration", new() { Exact = true }).IsVisibleAsync());

        await page.GotoAsync($"{BaseUrl}/infra-interface");
        Assert.False(await page.GetByText("Access Denied").IsVisibleAsync());
    }

    [UiFact]
    [Trait("Category", "UI")]
    public async Task Employee_Can_Login()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await LoginAsync(context, "thomas.miller@asteelflash.com", "password123");

        Assert.True(await page.Locator("a[href='/logout']").IsVisibleAsync());
        Assert.False(await page.GetByText("Administration", new() { Exact = true }).IsVisibleAsync());
        // new: employee should see a limited dashboard
        Assert.True(await page.GetByText("Employee Dashboard", new() { Exact = true }).IsVisibleAsync());
    }
}
