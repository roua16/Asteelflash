namespace ITStockM.Services.Interfaces
{
    /// <summary>
    /// Sends warranty-expiry alert emails for assets whose warranty ends within a threshold.
    /// Separated from <see cref="INotificationService"/> (ISP).
    /// </summary>
    public interface IWarrantyAlertService
    {
        /// <summary>
        /// Examines all materiels and emails alerts for those whose
        /// <c>Warranty</c> or calculated end-of-life falls within
        /// the configured look-ahead window (default 30 days).
        /// </summary>
        Task SendWarrantyExpiryAlertsAsync(CancellationToken ct = default);
    }
}
