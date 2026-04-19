using ITStockM.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ITStockM.Services.Implementation
{
    /// <summary>
    /// Service for handling application-wide notifications
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task NotifyMaterialAssignmentAsync(int assignmentId, string assignedBy, string assignedTo)
        {
            try
            {
                var action = "Material Assignment Created";
                var details = $@"
                    <p><strong>Assignment ID:</strong> {assignmentId}</p>
                    <p><strong>Assigned To:</strong> {assignedTo}</p>
                ";

                await _emailService.SendAdminNotificationAsync(action, details, assignedBy);
                _logger.LogInformation($"Notification sent for material assignment {assignmentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification for material assignment {assignmentId}");
            }
        }

        public async Task NotifyDeliveryOrderCreatedAsync(string orderNumber, string createdBy)
        {
            try
            {
                var action = "Delivery Order Created";
                var details = $@"
                    <p><strong>Order Number:</strong> {orderNumber}</p>
                ";

                await _emailService.SendAdminNotificationAsync(action, details, createdBy);
                _logger.LogInformation($"Notification sent for delivery order {orderNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification for delivery order {orderNumber}");
            }
        }

        public async Task NotifyRequestSubmittedAsync(int requestId, string requestedBy)
        {
            try
            {
                var action = "New Request Submitted";
                var details = $@"
                    <p><strong>Request ID:</strong> {requestId}</p>
                ";

                await _emailService.SendAdminNotificationAsync(action, details, requestedBy);
                _logger.LogInformation($"Notification sent for request {requestId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification for request {requestId}");
            }
        }

        public async Task NotifyMaterialChangeAsync(string action, int materialId, string performedBy)
        {
            try
            {
                var actionText = $"Material {action}";
                var details = $@"
                    <p><strong>Material ID:</strong> {materialId}</p>
                    <p><strong>Action:</strong> {action}</p>
                ";

                await _emailService.SendAdminNotificationAsync(actionText, details, performedBy);
                _logger.LogInformation($"Notification sent for material {action} - ID: {materialId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification for material {action}");
            }
        }

        public async Task NotifySupplierChangeAsync(string action, string supplierName, string performedBy)
        {
            try
            {
                var actionText = $"Supplier {action}";
                var details = $@"
                    <p><strong>Supplier Name:</strong> {supplierName}</p>
                    <p><strong>Action:</strong> {action}</p>
                ";

                await _emailService.SendAdminNotificationAsync(actionText, details, performedBy);
                _logger.LogInformation($"Notification sent for supplier {action} - Name: {supplierName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification for supplier {action}");
            }
        }

        public async Task NotifyProjectChangeAsync(string action, int projectId, string performedBy)
        {
            try
            {
                var actionText = $"Project {action}";
                var details = $@"
                    <p><strong>Project ID:</strong> {projectId}</p>
                    <p><strong>Action:</strong> {action}</p>
                ";

                await _emailService.SendAdminNotificationAsync(actionText, details, performedBy);
                _logger.LogInformation($"Notification sent for project {action} - ID: {projectId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification for project {action}");
            }
        }
    }
}
