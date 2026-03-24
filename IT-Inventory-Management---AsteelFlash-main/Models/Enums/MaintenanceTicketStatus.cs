namespace ITStockM.Models.Enums
{
    /// <summary>Represents the workflow status of a maintenance ticket.</summary>
    public static class MaintenanceTicketStatus
    {
        public const string Open       = "Open";
        public const string InProgress = "InProgress";
        public const string Closed     = "Closed";

        public static readonly IReadOnlyList<string> All = new[]
        {
            Open, InProgress, Closed
        };
    }
}
