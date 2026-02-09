using Microsoft.Playwright;
using Xunit;

namespace ITStockM.Tests.Ui;

public sealed class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var headless = !string.Equals(Environment.GetEnvironmentVariable("UI_HEADLESS"), "false", StringComparison.OrdinalIgnoreCase);
        var channel = Environment.GetEnvironmentVariable("PLAYWRIGHT_CHANNEL");
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = headless,
            Channel = string.IsNullOrWhiteSpace(channel) ? null : channel
        };

        try
        {
            Browser = await Playwright.Chromium.LaunchAsync(launchOptions);
            return;
        }
        catch (PlaywrightException ex) when (ex.Message.Contains("Executable doesn't exist", StringComparison.OrdinalIgnoreCase))
        {
            Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
        }

        try
        {
            Browser = await Playwright.Chromium.LaunchAsync(launchOptions);
            return;
        }
        catch (PlaywrightException)
        {
            var chromeOptions = new BrowserTypeLaunchOptions
            {
                Headless = headless,
                Channel = "chrome"
            };

            Browser = await Playwright.Chromium.LaunchAsync(chromeOptions);
        }
    }

    public async Task DisposeAsync()
    {
        if (Browser != null)
        {
            await Browser.CloseAsync();
        }

        Playwright?.Dispose();
    }
}
