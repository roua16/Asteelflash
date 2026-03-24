namespace ITStockM.Models.Enums
{
    /// <summary>Health status band derived from the computed health score.</summary>
    public static class HealthStatus
    {
        public const string Good = "Good";   // >= 75
        public const string Fair = "Fair";   // >= 45
        public const string Poor = "Poor";   // <  45

        public static string FromScore(decimal score) => score switch
        {
            >= 75 => Good,
            >= 45 => Fair,
            _     => Poor
        };
    }
}
