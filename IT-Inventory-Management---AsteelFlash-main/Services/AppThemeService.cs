using Microsoft.JSInterop;
using Radzen;

namespace ITStockM.Services;

public enum AppThemeMode
{
    Dark,
    Light
}


public sealed class AppThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ThemeService _radzenThemeService;
    private bool _initialized;

    public AppThemeService(IJSRuntime jsRuntime, ThemeService radzenThemeService)
    {
        _jsRuntime = jsRuntime;
        _radzenThemeService = radzenThemeService;
    }

    public event Action? ThemeChanged;

    public AppThemeMode CurrentMode { get; private set; } = AppThemeMode.Dark;

    public bool IsDark => CurrentMode == AppThemeMode.Dark;

    public string BodyClass => IsDark ? "theme-dark" : "theme-light";

    public string RadzenThemeName => IsDark ? "humanistic-dark" : "humanistic";

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        var storedTheme = await InvokeSafeAsync<string?>("themeManager.getStoredTheme");
        if (!TryParseMode(storedTheme, out var mode))
        {
            var systemTheme = await InvokeSafeAsync<string?>("themeManager.getSystemTheme");
            mode = TryParseMode(systemTheme, out var parsedSystemMode) ? parsedSystemMode : AppThemeMode.Dark;
        }

        await ApplyThemeAsync(mode, persist: false, notify: false);

        _initialized = true;
        ThemeChanged?.Invoke();
    }

    public Task ToggleAsync()
    {
        return SetThemeAsync(IsDark ? AppThemeMode.Light : AppThemeMode.Dark);
    }

    public Task SetThemeAsync(AppThemeMode mode)
    {
        return ApplyThemeAsync(mode, persist: true, notify: true);
    }

    private async Task ApplyThemeAsync(AppThemeMode mode, bool persist, bool notify)
    {
        CurrentMode = mode;

        try
        {
            _radzenThemeService.SetTheme(RadzenThemeName);
        }
        catch
        {
            // Radzen theme switch is best-effort; custom CSS variables still keep theme coherent.
        }

        await InvokeSafeVoidAsync("themeManager.applyTheme", IsDark ? "dark" : "light");

        if (persist)
        {
            await InvokeSafeVoidAsync("themeManager.setStoredTheme", IsDark ? "dark" : "light");
        }

        if (notify)
        {
            ThemeChanged?.Invoke();
        }
    }

    private static bool TryParseMode(string? rawValue, out AppThemeMode mode)
    {
        mode = AppThemeMode.Dark;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        var value = rawValue.Trim().ToLowerInvariant();
        if (value is "dark" or "humanistic-dark")
        {
            mode = AppThemeMode.Dark;
            return true;
        }

        if (value is "light" or "humanistic")
        {
            mode = AppThemeMode.Light;
            return true;
        }

        if (value.Contains("dark", StringComparison.Ordinal))
        {
            mode = AppThemeMode.Dark;
            return true;
        }

        if (value.Contains("light", StringComparison.Ordinal))
        {
            mode = AppThemeMode.Light;
            return true;
        }

        return false;
    }

    private async Task<T?> InvokeSafeAsync<T>(string identifier)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<T>(identifier);
        }
        catch (JSDisconnectedException)
        {
            return default;
        }
        catch (InvalidOperationException)
        {
            return default;
        }
        catch (JSException)
        {
            return default;
        }
    }

    private async Task InvokeSafeVoidAsync(string identifier, params object[]? args)
    {
        try
        {
            if (args is null || args.Length == 0)
            {
                await _jsRuntime.InvokeVoidAsync(identifier);
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync(identifier, args);
            }
        }
        catch (JSDisconnectedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSException)
        {
        }
    }
}