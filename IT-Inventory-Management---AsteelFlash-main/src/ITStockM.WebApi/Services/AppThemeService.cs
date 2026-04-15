using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace ITStockM.WebApi.Services;

public sealed class AppThemeService
{
    private const string DarkTheme = "dark";
    private const string LightTheme = "light";

    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AppThemeService> _logger;
    private Task? _initializeTask;

    private string _currentTheme = DarkTheme;
    private bool _isDark = true;

    public AppThemeService(IJSRuntime jsRuntime, ILogger<AppThemeService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public event Action? ThemeChanged;

    public string CurrentTheme => _currentTheme;
    public bool IsDark => _isDark;
    public string BodyClass => _isDark ? "theme-dark" : "theme-light";

    public Task InitializeAsync()
    {
        _initializeTask ??= InitializeCoreAsync();
        return _initializeTask;
    }

    public void SetTheme(string theme)
    {
        SetThemeInternal(NormalizeTheme(theme), raiseEvent: true);
    }

    public void ToggleTheme()
    {
        SetThemeInternal(_isDark ? LightTheme : DarkTheme, raiseEvent: true);
    }

    public async Task ToggleAsync()
    {
        var nextTheme = _isDark ? LightTheme : DarkTheme;
        SetThemeInternal(nextTheme, raiseEvent: true);
        await SyncBrowserThemeAsync(nextTheme);
    }

    private async Task InitializeCoreAsync()
    {
        var preferredTheme = await ReadBrowserThemeAsync();
        SetThemeInternal(preferredTheme, raiseEvent: true);
        await SyncBrowserThemeAsync(preferredTheme);
    }

    private async Task<string> ReadBrowserThemeAsync()
    {
        try
        {
            var theme = await _jsRuntime.InvokeAsync<string?>("getStoredThemePreference");
            return NormalizeTheme(theme);
        }
        catch (JSDisconnectedException)
        {
            _logger.LogDebug("JS disconnected while reading stored theme. Falling back to current theme.");
            return _currentTheme;
        }
        catch (InvalidOperationException ex) when (IsPrerenderingInteropException(ex))
        {
            _logger.LogDebug(ex, "Theme preference is not available during prerender.");
            return _currentTheme;
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "Unable to read theme from browser storage. Falling back to current theme.");
            return _currentTheme;
        }
    }

    private async Task SyncBrowserThemeAsync(string theme)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("setThemePreference", theme);
        }
        catch (JSDisconnectedException)
        {
            _logger.LogDebug("JS disconnected while applying theme.");
        }
        catch (InvalidOperationException ex) when (IsPrerenderingInteropException(ex))
        {
            _logger.LogDebug(ex, "Theme synchronization skipped during prerender.");
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "Unable to synchronize theme with browser.");
        }
    }

    private void SetThemeInternal(string theme, bool raiseEvent)
    {
        var isDark = string.Equals(theme, DarkTheme, StringComparison.Ordinal);
        if (string.Equals(_currentTheme, theme, StringComparison.Ordinal) && _isDark == isDark)
        {
            return;
        }

        _currentTheme = theme;
        _isDark = isDark;

        if (raiseEvent)
        {
            ThemeChanged?.Invoke();
        }
    }

    private static bool IsPrerenderingInteropException(InvalidOperationException exception)
    {
        return exception.Message.Contains("prerender", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("JavaScript interop calls cannot be issued", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeTheme(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DarkTheme;
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized == "humanistic-dark" || normalized == DarkTheme || normalized.Contains("dark", StringComparison.Ordinal))
        {
            return DarkTheme;
        }

        if (normalized == "humanistic" || normalized == LightTheme || normalized.Contains("light", StringComparison.Ordinal))
        {
            return LightTheme;
        }

        return DarkTheme;
    }
}
