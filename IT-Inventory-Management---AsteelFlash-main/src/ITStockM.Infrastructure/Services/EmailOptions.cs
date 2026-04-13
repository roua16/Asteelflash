namespace ITStockM.Services.Implementation
{
    /// <summary>
    /// Strongly-typed configuration for SMTP.
    /// Bind from appsettings.json section "EmailSettings"
    /// or from environment variables via the standard .NET provider bridging.
    /// </summary>
    public class EmailOptions
    {
        public const string Section = "EmailSettings";

        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int    SmtpPort   { get; set; } = 587;
        public string FromEmail  { get; set; } = string.Empty;
        public string Password   { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;

        /// <summary>Times to retry after a transient SMTP failure.</summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>Base delay in milliseconds between retries (doubles each attempt).</summary>
        public int RetryDelayMs { get; set; } = 1500;

        // ─── Helper: resolve env-var overrides at startup ───────────────────────
        public void ApplyEnvironmentOverrides()
        {
            SmtpServer = NotEmpty(Environment.GetEnvironmentVariable("SMTP_SERVER"),  SmtpServer);
            FromEmail  = NotEmpty(Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL"), FromEmail);
            Password   = NotEmpty(Environment.GetEnvironmentVariable("SMTP_PASSWORD"),   Password);
            AdminEmail = NotEmpty(Environment.GetEnvironmentVariable("SMTP_ADMIN_EMAIL"), AdminEmail.Length > 0 ? AdminEmail : FromEmail);

            var portStr = Environment.GetEnvironmentVariable("SMTP_PORT");
            if (int.TryParse(portStr, out var port))
                SmtpPort = port;
        }

        private static string NotEmpty(string? candidate, string fallback)
            => string.IsNullOrWhiteSpace(candidate) ? fallback : candidate;
    }
}
