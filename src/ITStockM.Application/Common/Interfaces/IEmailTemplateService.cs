namespace ITStockM.Services.Interfaces
{
    /// <summary>
    /// Responsible solely for building HTML email bodies (SRP).
    /// No sending occurs here — that is the concern of <see cref="IEmailService"/>.
    /// </summary>
    public interface IEmailTemplateService
    {
        string BuildActionNotification(string action, string details, string performedBy);

        string BuildWarrantyExpiryAlert(string materielName, string serialNumber, DateTime warrantyEnd, int daysRemaining);

        string BuildLowHealthScoreAlert(string materielName, string serialNumber, decimal healthScore, string healthStatus, string? recommendation);

        string BuildMaintenanceTicketCreated(int ticketId, string materielName, string problem, string reportedBy);

        string BuildMaintenanceTicketClosed(int ticketId, string materielName, string resolution, decimal? costEur);

        string BuildAssetReplacementForecast(IEnumerable<(string MaterielName, string SerialNumber, DateTime ReplacementDate, decimal? Cost)> assets);
    }
}
