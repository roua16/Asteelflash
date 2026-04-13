namespace ITStockM.Domain.Enums
{
    /// <summary>Represents the current stage of an IT asset in its lifecycle.</summary>
    public static class LifecycleStage
    {
        public const string Purchased       = "Purchased";
        public const string InStock         = "InStock";
        public const string Assigned        = "Assigned";
        public const string UnderMaintenance = "UnderMaintenance";
        public const string Retired         = "Retired";

        public static readonly IReadOnlyList<string> All = new[]
        {
            Purchased, InStock, Assigned, UnderMaintenance, Retired
        };
    }
}
