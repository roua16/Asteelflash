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

    public sealed record RoleScenario(string Role, string Email, string Password, string[] AllowedRoutes, string[] DeniedRoutes);

    private static readonly RoleScenario[] Scenarios =
    [
        new(
            "Admin",
            "admin@asteelflash.com",
            "admin1234",
            ["/projects-super-admin", "/purchase-page", "/asset-predictions"],
            []),
        new(
            "PDR",
            "pdr@asteelflash.com",
            "pdr1234",
            ["/standby", "/suppliers", "/delivery-order"],
            ["/purchase-page"]),
        new(
            "Purchasing",
            "purchasing@asteelflash.com",
            "purchasing1234",
            ["/purchase-page", "/suppliers", "/archived-requests-page"],
            ["/asset-predictions"]),
        new(
            "IT",
            "it@asteelflash.com",
            "it1234",
            ["/assignments-interface", "/infra-interface", "/asset-predictions"],
            ["/purchase-page"]),
        new(
            "Infrastructure",
            "infrastructure@asteelflash.com",
            "infrastructure1234",
            ["/assignments-interface", "/infra-interface", "/asset-predictions"],
            ["/purchase-page"]),
        new(
            "Employee",
            "employee@asteelflash.com",
            "employee1234",
            ["/dashboard", "/assignments-interface", "/standby"],
            ["/suppliers"])
    ];

    public static IEnumerable<object[]> RoleScenarios() => Scenarios.Select(s => new object[] { s });

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
        return page;
    }

    private static async Task<bool> IsAccessDeniedAsync(IPage page)
    {
        if (page.Url.Contains("/access-denied", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var deniedBanner = page.GetByText("Access Denied", new() { Exact = false });
        return await deniedBanner.IsVisibleAsync();
    }

    private static async Task AssertAccessAsync(IPage page, string route, bool shouldAllow, string role)
    {
        await page.GotoAsync($"{BaseUrl}{route}");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var denied = await IsAccessDeniedAsync(page);
        if (shouldAllow)
        {
            Assert.False(denied, $"Role '{role}' should be allowed on '{route}' but was denied.");
            return;
        }

        Assert.True(denied, $"Role '{role}' should be denied on '{route}' but access was granted.");
    }

    [UiTheory]
    [Trait("Category", "UI")]
    [MemberData(nameof(RoleScenarios))]
    public async Task Role_Access_Matrix_Is_Enforced(RoleScenario scenario)
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await LoginAsync(context, scenario.Email, scenario.Password);

        Assert.True(await page.Locator("a[href='/logout']").IsVisibleAsync());
        await AssertAccessAsync(page, "/dashboard", shouldAllow: true, scenario.Role);

        foreach (var route in scenario.AllowedRoutes)
        {
            await AssertAccessAsync(page, route, shouldAllow: true, scenario.Role);
        }

        foreach (var route in scenario.DeniedRoutes)
        {
            await AssertAccessAsync(page, route, shouldAllow: false, scenario.Role);
        }
    }
}
