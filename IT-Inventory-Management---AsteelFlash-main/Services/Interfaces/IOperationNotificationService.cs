using ITStockM.Models.ITStockManagment;

namespace ITStockM.Services.Interfaces
{
    /// <summary>
    /// Interface for sending email notifications when operations are performed in the system
    /// </summary>
    public interface IOperationNotificationService
    {
        // Assignment notifications
        Task NotifyAssignmentCreated(Assignment assignment, string? performedBy = null);
        Task NotifyAssignmentUpdated(Assignment assignment, string? performedBy = null);
        Task NotifyAssignmentDeleted(int assignmentId, string? performedBy = null);

        // Materiel notifications
        Task NotifyMaterielCreated(Materiel materiel, string? performedBy = null);
        Task NotifyMaterielUpdated(Materiel materiel, string? performedBy = null);
        Task NotifyMaterielDeleted(Materiel materiel, string? performedBy = null);

        // DeliveryOrder notifications
        Task NotifyDeliveryOrderCreated(DeliveryOrder deliveryOrder, string? performedBy = null);
        Task NotifyDeliveryOrderUpdated(DeliveryOrder deliveryOrder, string? performedBy = null);
        Task NotifyDeliveryOrderDeleted(string deliveryOrderNumber, string? performedBy = null);

        // Request notifications
        Task NotifyRequestCreated(Request request, string? performedBy = null);
        Task NotifyRequestUpdated(Request request, string? performedBy = null);
        Task NotifyRequestDeleted(int requestId, string? performedBy = null);

        // Offer notifications
        Task NotifyOfferCreated(Offer offer, string? performedBy = null);
        Task NotifyOfferUpdated(Offer offer, string? performedBy = null);
        Task NotifyOfferDeleted(int offerId, string? performedBy = null);

        // Supplier notifications
        Task NotifySupplierCreated(Supplier supplier, string? performedBy = null);
        Task NotifySupplierUpdated(Supplier supplier, string? performedBy = null);
        Task NotifySupplierDeleted(string supplierName, string? performedBy = null);

        // Assignment return issue notification (e.g., missing/damaged returns)
        Task NotifyAssignmentReturnIssue(Assignment assignment, string issueDetails, IEnumerable<string>? recipients = null);

        // Low stock notification (single materiel)
        Task NotifyMaterielLowStock(Materiel materiel, int threshold, string? performedBy = null);

        // Low stock summary (sent once-per-day per recipient when requested)
        Task NotifyLowStockSummary(IEnumerable<Materiel> lowMateriels, string toEmail, string? performedBy = null);

        // Daily top-used materials notification (sent to a user once per day when requested)
        Task NotifyTopUsedMateriels(IEnumerable<ITStockM.Models.ViewModels.MaterielUsageViewModel> topMateriels, string toEmail, string? performedBy = null);
    }
}
