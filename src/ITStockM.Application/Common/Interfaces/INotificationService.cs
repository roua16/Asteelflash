namespace ITStockM.Services.Interfaces
{
    /// <summary>
    /// Interface for handling application-wide notifications
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Notifies admin when a material is assigned
        /// </summary>
        Task NotifyMaterialAssignmentAsync(int assignmentId, string assignedBy, string assignedTo);

        /// <summary>
        /// Notifies admin when a delivery order is created
        /// </summary>
        Task NotifyDeliveryOrderCreatedAsync(string orderNumber, string createdBy);

        /// <summary>
        /// Notifies admin when a request is submitted
        /// </summary>
        Task NotifyRequestSubmittedAsync(int requestId, string requestedBy);

        /// <summary>
        /// Notifies admin when a material is added or updated
        /// </summary>
        Task NotifyMaterialChangeAsync(string action, int materialId, string performedBy);

        /// <summary>
        /// Notifies admin when a supplier is added or updated
        /// </summary>
        Task NotifySupplierChangeAsync(string action, string supplierName, string performedBy);

        /// <summary>
        /// Notifies admin when a project is created or updated
        /// </summary>
        Task NotifyProjectChangeAsync(string action, int projectId, string performedBy);
    }
}
