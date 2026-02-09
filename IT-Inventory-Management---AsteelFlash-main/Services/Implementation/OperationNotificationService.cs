using ITStockM.Models.ITStockManagment;
using ITStockM.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ITStockM.Services.Implementation
{
    /// <summary>
    /// Service for sending email notifications when operations are performed in the system
    /// </summary>
    public class OperationNotificationService : IOperationNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<OperationNotificationService> _logger;
        private readonly string _adminEmail;
        private readonly Data.ITStockManagmentContext _context;

        public OperationNotificationService(
            IEmailService emailService,
            ILogger<OperationNotificationService> logger,
            Data.ITStockManagmentContext context)
        {
            _emailService = emailService;
            _logger = logger;
            _context = context;
            _adminEmail = Environment.GetEnvironmentVariable("SMTP_ADMIN_EMAIL") ?? "admin@asteelflash.com";
        }

        #region Assignment Notifications

        public async Task NotifyAssignmentCreated(Assignment assignment, string? performedBy = null)
        {
            try
            {
                var subject = "🆕 New Assignment Created";
                var body = BuildAssignmentEmail("created", assignment, performedBy);
                await SendNotificationAsync(subject, body, "Assignment Created", assignment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send assignment created notification for AssignmentId={Id}", assignment.Id);
            }
        }

        public async Task NotifyAssignmentUpdated(Assignment assignment, string? performedBy = null)
        {
            try
            {
                var subject = "✏️ Assignment Updated";
                var body = BuildAssignmentEmail("updated", assignment, performedBy);
                await SendNotificationAsync(subject, body, "Assignment Updated", assignment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send assignment updated notification for AssignmentId={Id}", assignment.Id);
            }
        }

        public async Task NotifyAssignmentDeleted(int assignmentId, string? performedBy = null)
        {
            try
            {
                var subject = "🗑️ Assignment Deleted";
                var body = BuildDeleteEmail("Assignment", assignmentId.ToString(), performedBy);
                await SendNotificationAsync(subject, body, "Assignment Deleted", assignmentId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send assignment deleted notification for AssignmentId={Id}", assignmentId);
            }
        }

        private string BuildAssignmentEmail(string action, Assignment assignment, string? performedBy)
        {
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 10px; padding: 20px;'>
                    <h2 style='color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px;'>
                        Assignment {action.ToUpper()}
                    </h2>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold;'>Assignment ID:</td><td style='padding: 8px;'>{assignment.Id}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Date:</td><td style='padding: 8px;'>{assignment.Date:yyyy-MM-dd HH:mm}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Description:</td><td style='padding: 8px;'>{assignment.Descipriton ?? "N/A"}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>On Mission:</td><td style='padding: 8px;'>{(assignment.OnMission ? "Yes" : "No")}</td></tr>
                        {(assignment.RestoreDateLimit.HasValue ? $"<tr><td style='padding: 8px; font-weight: bold;'>Return Date Limit:</td><td style='padding: 8px;'>{assignment.RestoreDateLimit:yyyy-MM-dd}</td></tr>" : "")}
                    </table>
                    {GetPerformedBySection(performedBy)}
                    {GetFooter()}
                </div>
            </body>
            </html>";
        }

        #endregion

        #region Materiel Notifications

        public async Task NotifyMaterielCreated(Materiel materiel, string? performedBy = null)
        {
            try
            {
                var subject = "🆕 New Material Added";
                var body = BuildMaterielEmail("added", materiel, performedBy);
                await SendNotificationAsync(subject, body, "Materiel Created", materiel.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send materiel created notification for MaterielId={Id}", materiel.Id);
            }
        }

        public async Task NotifyMaterielUpdated(Materiel materiel, string? performedBy = null)
        {
            try
            {
                var subject = "✏️ Material Updated";
                var body = BuildMaterielEmail("updated", materiel, performedBy);
                await SendNotificationAsync(subject, body, "Materiel Updated", materiel.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send materiel updated notification for MaterielId={Id}", materiel.Id);
            }
        }

        public async Task NotifyMaterielDeleted(Materiel materiel, string? performedBy = null)
        {
            try
            {
                var subject = "🗑️ Material Deleted";
                var body = BuildDeleteEmail("Material", materiel.Id.ToString(), performedBy);
                await SendNotificationAsync(subject, body, "Materiel Deleted", materiel.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send materiel deleted notification for MaterielId={Id}", materiel.Id);
            }
        }

        private string BuildMaterielEmail(string action, Materiel materiel, string? performedBy)
        {
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 10px; padding: 20px;'>
                    <h2 style='color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px;'>
                        Material {action.ToUpper()}
                    </h2>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold;'>Material ID:</td><td style='padding: 8px;'>{materiel.Id}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Name:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(materiel.MaterielName)}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Type:</td><td style='padding: 8px;'>{materiel.Type}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Serial Number:</td><td style='padding: 8px;'>{materiel.SerialNumber ?? "N/A"}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>IT Stock Qty:</td><td style='padding: 8px;'>{materiel.QuantityITStock}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>PDR Stock Qty:</td><td style='padding: 8px;'>{materiel.QuantityPDRStock}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Irreparable Qty:</td><td style='padding: 8px;'>{materiel.IrreparableQuantity}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Repairing Qty:</td><td style='padding: 8px;'>{materiel.Repairing_Quantity}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Warranty:</td><td style='padding: 8px;'>{materiel.Warranty:yyyy-MM-dd}</td></tr>
                    </table>
                    {GetPerformedBySection(performedBy)}
                    {GetFooter()}
                </div>
            </body>
            </html>";
        }


        // New low stock notifier
        public async Task NotifyMaterielLowStock(Materiel materiel, int threshold, string? performedBy = null)
        {
            try
            {
                var subject = $"⚠️ Low Stock Alert: {materiel.MaterielName}";
                var body = BuildLowStockEmail(materiel, threshold, performedBy);

                // Build recipient list from env vars or seeded employees
                var recipientsList = new List<string>();
                var pdrEmail = Environment.GetEnvironmentVariable("SMTP_PDR_EMAIL");
                var itEmail = Environment.GetEnvironmentVariable("SMTP_IT_EMAIL");

                if (!string.IsNullOrWhiteSpace(pdrEmail)) recipientsList.Add(pdrEmail);
                if (!string.IsNullOrWhiteSpace(itEmail)) recipientsList.Add(itEmail);

                if (!recipientsList.Any())
                {
                    // Query seeded employees with roles PDR/IT/Admin
                    var employees = _context.Employees.Where(e => e.Role == Models.Constants.UserRoles.PDR || e.Role == Models.Constants.UserRoles.IT || e.Role == Models.Constants.UserRoles.Admin)
                                        .Select(e => e.Email)
                                        .Where(e => !string.IsNullOrWhiteSpace(e))
                                        .Distinct()
                                        .ToList();

                    recipientsList.AddRange(employees);
                }

                if (!recipientsList.Any())
                {
                    recipientsList.Add(_adminEmail);
                }

                recipientsList = recipientsList.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                _logger.LogInformation("Sending low stock notification to: {Recipients} for MaterielId={MaterielId}", string.Join(',', recipientsList), materiel.Id);
                await _emailService.SendEmailToMultipleAsync(recipientsList, subject, body);
                _logger.LogInformation("Low stock notification sent successfully for MaterielId={MaterielId}", materiel.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send low stock notification for MaterielId={Id}", materiel.Id);
            }
        }

        private string BuildLowStockEmail(Materiel materiel, int threshold, string? performedBy)
        {
            var total = materiel.QuantityITStock + materiel.QuantityPDRStock;
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <div style='max-width: 600px; margin: 0 auto; background: #fff9e6; border-radius: 10px; padding: 20px;'>
                        <h2 style='color: #e67e22; border-bottom: 2px solid #e67e22; padding-bottom: 10px;'>
                            Low Stock Alert: {System.Net.WebUtility.HtmlEncode(materiel.MaterielName)}
                        </h2>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr><td style='padding: 8px; font-weight: bold;'>Material ID:</td><td style='padding: 8px;'>{materiel.Id}</td></tr>
                            <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Name:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(materiel.MaterielName)}</td></tr>
                            <tr><td style='padding: 8px; font-weight: bold;'>IT Stock Qty:</td><td style='padding: 8px;'>{materiel.QuantityITStock}</td></tr>
                            <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>PDR Stock Qty:</td><td style='padding: 8px;'>{materiel.QuantityPDRStock}</td></tr>
                            <tr><td style='padding: 8px; font-weight: bold;'>Total Available:</td><td style='padding: 8px;'>{total}</td></tr>
                            <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Threshold:</td><td style='padding: 8px;'>{threshold}</td></tr>
                        </table>
                        {GetPerformedBySection(performedBy)}
                        {GetFooter()}
                    </div>
                </body>
                </html>";
        }

        #endregion

        #region DeliveryOrder Notifications

        public async Task NotifyDeliveryOrderCreated(DeliveryOrder deliveryOrder, string? performedBy = null)
        {
            try
            {
                var subject = "🆕 New Delivery Order Created";
                var body = BuildDeliveryOrderEmail("created", deliveryOrder, performedBy);
                await SendNotificationAsync(subject, body, "DeliveryOrder Created", deliveryOrder.DeleveryOrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send delivery order created notification for OrderNumber={Number}", deliveryOrder.DeleveryOrderNumber);
            }
        }

        public async Task NotifyDeliveryOrderUpdated(DeliveryOrder deliveryOrder, string? performedBy = null)
        {
            try
            {
                var subject = "✏️ Delivery Order Updated";
                var body = BuildDeliveryOrderEmail("updated", deliveryOrder, performedBy);
                await SendNotificationAsync(subject, body, "DeliveryOrder Updated", deliveryOrder.DeleveryOrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send delivery order updated notification for OrderNumber={Number}", deliveryOrder.DeleveryOrderNumber);
            }
        }

        public async Task NotifyDeliveryOrderDeleted(string deliveryOrderNumber, string? performedBy = null)
        {
            try
            {
                var subject = "🗑️ Delivery Order Deleted";
                var body = BuildDeleteEmail("Delivery Order", deliveryOrderNumber, performedBy);
                await SendNotificationAsync(subject, body, "DeliveryOrder Deleted", deliveryOrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send delivery order deleted notification for OrderNumber={Number}", deliveryOrderNumber);
            }
        }

        private string BuildDeliveryOrderEmail(string action, DeliveryOrder deliveryOrder, string? performedBy)
        {
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 10px; padding: 20px;'>
                    <h2 style='color: #2c3e50; border-bottom: 2px solid #e74c3c; padding-bottom: 10px;'>
                        Delivery Order {action.ToUpper()}
                    </h2>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold;'>Delivery Order #:</td><td style='padding: 8px;'>{deliveryOrder.DeleveryOrderNumber}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Order Number:</td><td style='padding: 8px;'>{deliveryOrder.OrderNumber ?? "Not Set"}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Date:</td><td style='padding: 8px;'>{deliveryOrder.Date:yyyy-MM-dd}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Supplier:</td><td style='padding: 8px;'>{deliveryOrder.SupplierName}</td></tr>
                        {(deliveryOrder.DeliveryDate.HasValue ? $"<tr><td style='padding: 8px; font-weight: bold;'>Delivery Date:</td><td style='padding: 8px;'>{deliveryOrder.DeliveryDate:yyyy-MM-dd}</td></tr>" : "")}
                        <tr><td style='padding: 8px; font-weight: bold;'>Description:</td><td style='padding: 8px;'>{deliveryOrder.Descriptoin ?? "N/A"}</td></tr>
                    </table>
                    {GetPerformedBySection(performedBy)}
                    {GetFooter()}
                </div>
            </body>
            </html>";
        }

        #endregion

        #region Request Notifications

        public async Task NotifyRequestCreated(Request request, string? performedBy = null)
        {
            try
            {
                var subject = "🆕 New Request Created";
                var body = BuildRequestEmail("created", request, performedBy);
                await SendNotificationAsync(subject, body, "Request Created", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send request created notification for RequestId={Id}", request.Id);
            }
        }

        public async Task NotifyRequestUpdated(Request request, string? performedBy = null)
        {
            try
            {
                var subject = $"✏️ Request Updated - Status: {request.Status}";
                var body = BuildRequestEmail("updated", request, performedBy);
                await SendNotificationAsync(subject, body, "Request Updated", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send request updated notification for RequestId={Id}", request.Id);
            }
        }

        public async Task NotifyRequestDeleted(int requestId, string? performedBy = null)
        {
            try
            {
                var subject = "🗑️ Request Deleted";
                var body = BuildDeleteEmail("Request", requestId.ToString(), performedBy);
                await SendNotificationAsync(subject, body, "Request Deleted", requestId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send request deleted notification for RequestId={Id}", requestId);
            }
        }

        private string BuildRequestEmail(string action, Request request, string? performedBy)
        {
            var statusColor = request.Status?.ToLower() switch
            {
                "done" => "#27ae60",
                "pending" => "#f39c12",
                "rejected" => "#e74c3c",
                _ => "#3498db"
            };

            return $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 10px; padding: 20px;'>
                    <h2 style='color: #2c3e50; border-bottom: 2px solid #9b59b6; padding-bottom: 10px;'>
                        Request {action.ToUpper()}
                    </h2>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold;'>Request ID:</td><td style='padding: 8px;'>{request.Id}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Title:</td><td style='padding: 8px;'>{request.Title}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Status:</td><td style='padding: 8px;'><span style='background: {statusColor}; color: white; padding: 2px 8px; border-radius: 4px;'>{request.Status}</span></td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Material Type:</td><td style='padding: 8px;'>{request.MaterialType ?? "N/A"}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Project:</td><td style='padding: 8px;'>{request.ProjectName ?? "N/A"}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Date:</td><td style='padding: 8px;'>{request.Date:yyyy-MM-dd HH:mm}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Description:</td><td style='padding: 8px;'>{request.Description ?? "N/A"}</td></tr>
                    </table>
                    {GetPerformedBySection(performedBy)}
                    {GetFooter()}
                </div>
            </body>
            </html>";
        }

        #endregion

        #region Offer Notifications

        public async Task NotifyOfferCreated(Offer offer, string? performedBy = null)
        {
            try
            {
                var subject = "🆕 New Offer Added";
                var body = BuildOfferEmail("added", offer, performedBy);
                await SendNotificationAsync(subject, body, "Offer Created", offer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send offer created notification for OfferId={Id}", offer.Id);
            }
        }

        public async Task NotifyOfferUpdated(Offer offer, string? performedBy = null)
        {
            try
            {
                var subject = offer.Selected == true ? "✅ Offer Selected" : "✏️ Offer Updated";
                var body = BuildOfferEmail(offer.Selected == true ? "selected" : "updated", offer, performedBy);
                await SendNotificationAsync(subject, body, "Offer Updated", offer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send offer updated notification for OfferId={Id}", offer.Id);
            }
        }

        public async Task NotifyOfferDeleted(int offerId, string? performedBy = null)
        {
            try
            {
                var subject = "🗑️ Offer Deleted";
                var body = BuildDeleteEmail("Offer", offerId.ToString(), performedBy);
                await SendNotificationAsync(subject, body, "Offer Deleted", offerId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send offer deleted notification for OfferId={Id}", offerId);
            }
        }

        private string BuildOfferEmail(string action, Offer offer, string? performedBy)
        {
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 10px; padding: 20px;'>
                    <h2 style='color: #2c3e50; border-bottom: 2px solid #f39c12; padding-bottom: 10px;'>
                        Offer {action.ToUpper()}
                    </h2>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold;'>Offer ID:</td><td style='padding: 8px;'>{offer.Id}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Request ID:</td><td style='padding: 8px;'>{offer.RequestId}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Supplier:</td><td style='padding: 8px;'>{offer.SupplierName}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Price:</td><td style='padding: 8px;'>{offer.Price:C}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Delivery Date:</td><td style='padding: 8px;'>{offer.DeliveryDate:yyyy-MM-dd}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Selected:</td><td style='padding: 8px;'>{(offer.Selected == true ? "<span style='color: green; font-weight: bold;'>✓ Yes</span>" : "No")}</td></tr>
                    </table>
                    {GetPerformedBySection(performedBy)}
                    {GetFooter()}
                </div>
            </body>
            </html>";
        }

        #endregion

        #region Supplier Notifications

        public async Task NotifySupplierCreated(Supplier supplier, string? performedBy = null)
        {
            try
            {
                var subject = "🆕 New Supplier Added";
                var body = BuildSupplierEmail("added", supplier, performedBy);
                await SendNotificationAsync(subject, body, "Supplier Created", supplier.SupplierName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send supplier created notification for Supplier={Name}", supplier.SupplierName);
            }
        }

        public async Task NotifySupplierUpdated(Supplier supplier, string? performedBy = null)
        {
            try
            {
                var subject = "✏️ Supplier Updated";
                var body = BuildSupplierEmail("updated", supplier, performedBy);
                await SendNotificationAsync(subject, body, "Supplier Updated", supplier.SupplierName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send supplier updated notification for Supplier={Name}", supplier.SupplierName);
            }
        }

        public async Task NotifySupplierDeleted(string supplierName, string? performedBy = null)
        {
            try
            {
                var subject = "🗑️ Supplier Deleted";
                var body = BuildDeleteEmail("Supplier", supplierName, performedBy);
                await SendNotificationAsync(subject, body, "Supplier Deleted", supplierName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send supplier deleted notification for Supplier={Name}", supplierName);
            }
        }

        private string BuildSupplierEmail(string action, Supplier supplier, string? performedBy)
        {
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 10px; padding: 20px;'>
                    <h2 style='color: #2c3e50; border-bottom: 2px solid #1abc9c; padding-bottom: 10px;'>
                        Supplier {action.ToUpper()}
                    </h2>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold;'>Supplier Name:</td><td style='padding: 8px;'>{supplier.SupplierName}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Email:</td><td style='padding: 8px;'>{supplier.Email ?? "N/A"}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Phone:</td><td style='padding: 8px;'>{supplier.PhoneNumber ?? "N/A"}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Address:</td><td style='padding: 8px;'>{supplier.Adress ?? "N/A"}</td></tr>
                    </table>
                    {GetPerformedBySection(performedBy)}
                    {GetFooter()}
                </div>
            </body>
            </html>";
        }

        #endregion

        #region Helper Methods

        private string BuildDeleteEmail(string entityType, string entityId, string? performedBy)
        {
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 10px; padding: 20px;'>
                    <h2 style='color: #e74c3c; border-bottom: 2px solid #e74c3c; padding-bottom: 10px;'>
                        ⚠️ {entityType} DELETED
                    </h2>
                    <p style='font-size: 16px;'>A <strong>{entityType}</strong> has been deleted from the system.</p>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold;'>{entityType} ID:</td><td style='padding: 8px;'>{entityId}</td></tr>
                        <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Deleted At:</td><td style='padding: 8px;'>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</td></tr>
                    </table>
                    {GetPerformedBySection(performedBy)}
                    {GetFooter()}
                </div>
            </body>
            </html>";
        }

        private string GetPerformedBySection(string? performedBy)
        {
            if (string.IsNullOrEmpty(performedBy))
                return "";

            return $@"
                <div style='margin-top: 15px; padding: 10px; background: #fff; border-left: 4px solid #3498db;'>
                    <strong>Performed By:</strong> {performedBy}
                </div>";
        }

        private string GetFooter()
        {
            return $@"
                <div style='margin-top: 20px; padding-top: 15px; border-top: 1px solid #ddd; text-align: center; color: #7f8c8d; font-size: 12px;'>
                    <p>IT Stock Management System</p>
                    <p>This is an automated notification - {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                </div>";
        }

        private async Task SendNotificationAsync(string subject, string body, string operation, object entityId)
        {
            _logger.LogInformation("Sending operation notification: {Operation} for EntityId={EntityId}", operation, entityId);
            await _emailService.SendEmailAsync(_adminEmail, subject, body);
            _logger.LogInformation("Operation notification sent successfully: {Operation}", operation);
        }

        /// <summary>
        /// Notify a list of recipients about an assignment return issue (missing/damaged materials, etc.)
        /// </summary>
        public async Task NotifyAssignmentReturnIssue(Assignment assignment, string issueDetails, IEnumerable<string>? recipients = null)
        {
            try
            {
                var subject = "⚠️ Assignment Return Issue - Action Required";

                var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <div style='max-width: 600px; margin: 0 auto; background: #fff; border-radius: 10px; padding: 20px;'>
                        <h2 style='color: #e74c3c; border-bottom: 2px solid #e74c3c; padding-bottom: 10px;'>
                            Assignment Return Issue
                        </h2>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr><td style='padding: 8px; font-weight: bold;'>Assignment ID:</td><td style='padding: 8px;'>{assignment.Id}</td></tr>
                            <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Assigned To (ID):</td><td style='padding: 8px;'>{assignment.AssignedTo}</td></tr>
                            <tr><td style='padding: 8px; font-weight: bold;'>Date:</td><td style='padding: 8px;'>{assignment.Date:yyyy-MM-dd HH:mm}</td></tr>
                            <tr style='background: #fff;'><td style='padding: 8px; font-weight: bold;'>Description:</td><td style='padding: 8px;'>{assignment.Descipriton ?? "N/A"}</td></tr>
                        </table>
                        <div style='margin-top:15px; padding: 10px; background: #fff; border-left: 4px solid #e74c3c;'>
                            <strong>Issue Details:</strong>
                            <pre style='white-space: pre-wrap; font-family: inherit; background: #fafafa; padding: 10px; border-radius: 6px;'>{System.Net.WebUtility.HtmlEncode(issueDetails)}</pre>
                        </div>
                        {GetFooter()}
                    </div>
                </body>
                </html>";

                // Build final recipient list: explicit recipients + env fallback
                var recipientsList = new List<string>();

                if (recipients != null)
                    recipientsList.AddRange(recipients.Where(r => !string.IsNullOrWhiteSpace(r)));

                var pdrEmail = Environment.GetEnvironmentVariable("SMTP_PDR_EMAIL");
                var itEmail = Environment.GetEnvironmentVariable("SMTP_IT_EMAIL");

                if (!string.IsNullOrWhiteSpace(pdrEmail)) recipientsList.Add(pdrEmail);
                if (!string.IsNullOrWhiteSpace(itEmail)) recipientsList.Add(itEmail);

                // Ensure at least admin is included
                if (!recipientsList.Any())
                {
                    recipientsList.Add(_adminEmail);
                }

                recipientsList = recipientsList.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                _logger.LogInformation("Sending assignment return issue notification to: {Recipients}", string.Join(',', recipientsList));
                await _emailService.SendEmailToMultipleAsync(recipientsList, subject, body);
                _logger.LogInformation("Assignment return issue notifications sent successfully for AssignmentId={AssignmentId}", assignment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send assignment return issue notification for AssignmentId={Id}", assignment.Id);
            }
        }

        #endregion
    }
}
