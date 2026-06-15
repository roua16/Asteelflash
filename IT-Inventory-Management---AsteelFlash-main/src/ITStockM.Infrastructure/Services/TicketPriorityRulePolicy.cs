using ITStockM.Application.Features.Maintenance.DTOs;

namespace ITStockM.Services.Maintenance;

public static class TicketPriorityRulePolicy
{
    // Must match the training label inference and runtime fallback to ensure consistent behavior.
    private static readonly Dictionary<string, decimal> KeywordWeights = new(StringComparer.OrdinalIgnoreCase)
    {
        ["down"] = 30m,
        ["inaccessible"] = 30m,
        ["server"] = 20m,
        ["production"] = 20m,
        ["security"] = 20m,
        ["breach"] = 25m,
        ["urgent"] = 18m,
        ["critical"] = 22m,
        ["cannot"] = 12m,
        ["failed"] = 12m,
        ["crash"] = 15m,
        ["slow"] = 6m,
        ["printer"] = 4m,
        ["mouse"] = 2m,
        ["keyboard"] = 2m
    };

    public static string InferLabelFromText(string problemDescription)
    {
        var text = problemDescription?.ToLowerInvariant() ?? string.Empty;

        if (text.Contains("server") && (text.Contains("down") || text.Contains("inaccessible") || text.Contains("cannot")))
            return "Critique";

        if (text.Contains("critical") || text.Contains("urgent") || text.Contains("crash") || text.Contains("failed"))
            return "Haute";

        if (text.Contains("slow") || text.Contains("unstable") || text.Contains("intermittent"))
            return "Moyenne";

        return "Faible";
    }

    public static string DetermineFallbackPriority(
        IReadOnlyList<string> matchedKeywords,
        TicketPriorityRequestDto request)
    {
        var keywordScore = matchedKeywords.Sum(k => KeywordWeights[k]);
        var urgencyScore = Math.Clamp(request.UrgencyLevel, 1, 5) * 10m;
        var impactScore = (decimal)Math.Log10(Math.Max(request.ImpactedUsers, 1)) * 5m;
        var criticalityScore = (Math.Clamp(request.EquipmentCriticality, 1, 5) - 3) * 6m;

        var total = keywordScore + urgencyScore + impactScore + criticalityScore;

        return total switch
        {
            >= 95m => "Critique",
            >= 70m => "Haute",
            >= 45m => "Moyenne",
            _ => "Faible",
        };
    }

    public static decimal ComputeKeywordScore(IReadOnlyList<string> matchedKeywords)
        => matchedKeywords.Sum(k => KeywordWeights[k]);

    public static IReadOnlyDictionary<string, decimal> KeywordWeightsPublic => KeywordWeights;
}


