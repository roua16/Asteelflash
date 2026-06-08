using Xunit;

namespace ITStockM.Tests.Ui;

public sealed class UiTheoryAttribute : TheoryAttribute
{
    public UiTheoryAttribute()
    {
        var enabled = string.Equals(Environment.GetEnvironmentVariable("RUN_UI_TESTS"), "true", StringComparison.OrdinalIgnoreCase);
        if (!enabled)
        {
            Skip = "UI tests are disabled. Set RUN_UI_TESTS=true to enable.";
        }
    }
}
