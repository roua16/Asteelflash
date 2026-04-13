
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ITStockM.Data;

using Radzen;
using ITStockM.Services;

// EL kolhom 0 references hmmmmmmm sus
namespace ITStockM.Controllers
{
    public partial class ExportITStockManagmentController : ExportController
    {
        private readonly ITStockManagmentContext context;
        private readonly ITStockManagmentService service;

        public ExportITStockManagmentController(ITStockManagmentContext context, ITStockManagmentService service)
        {
            this.service = service;
            this.context = context;
        }

        [HttpGet("/export/ITStockManagment/assignments/csv")]
        [HttpGet("/export/ITStockManagment/assignments/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAssignmentsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetAssignments(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/assignments/excel")]
        [HttpGet("/export/ITStockManagment/assignments/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAssignmentsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetAssignments(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/assignmentmateriels/csv")]
        [HttpGet("/export/ITStockManagment/assignmentmateriels/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAssignmentMaterielsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetAssignmentMateriels(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/assignmentmateriels/excel")]
        [HttpGet("/export/ITStockManagment/assignmentmateriels/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAssignmentMaterielsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetAssignmentMateriels(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/deliveryorders/csv")]
        [HttpGet("/export/ITStockManagment/deliveryorders/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportDeliveryOrdersToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetDeliveryOrders(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/deliveryorders/excel")]
        [HttpGet("/export/ITStockManagment/deliveryorders/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportDeliveryOrdersToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetDeliveryOrders(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/deliveryordermateriels/csv")]
        [HttpGet("/export/ITStockManagment/deliveryordermateriels/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportDeliveryOrderMaterielsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetDeliveryOrderMateriels(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/deliveryordermateriels/excel")]
        [HttpGet("/export/ITStockManagment/deliveryordermateriels/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportDeliveryOrderMaterielsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetDeliveryOrderMateriels(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/employees/csv")]
        [HttpGet("/export/ITStockManagment/employees/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportEmployeesToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetEmployees(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/employees/excel")]
        [HttpGet("/export/ITStockManagment/employees/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportEmployeesToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetEmployees(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/materiels/csv")]
        [HttpGet("/export/ITStockManagment/materiels/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportMaterielsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetMateriels(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/materiels/excel")]
        [HttpGet("/export/ITStockManagment/materiels/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportMaterielsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetMateriels(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/offers/csv")]
        [HttpGet("/export/ITStockManagment/offers/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportOffersToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetOffers(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/offers/excel")]
        [HttpGet("/export/ITStockManagment/offers/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportOffersToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetOffers(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/projects/csv")]
        [HttpGet("/export/ITStockManagment/projects/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportProjectsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetProjects(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/projects/excel")]
        [HttpGet("/export/ITStockManagment/projects/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportProjectsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetProjects(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/requests/csv")]
        [HttpGet("/export/ITStockManagment/requests/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportRequestsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetRequests(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/requests/excel")]
        [HttpGet("/export/ITStockManagment/requests/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportRequestsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetRequests(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/suppliers/csv")]
        [HttpGet("/export/ITStockManagment/suppliers/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportSuppliersToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetSuppliers(), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/suppliers/excel")]
        [HttpGet("/export/ITStockManagment/suppliers/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportSuppliersToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetSuppliers(), Request.Query, false), fileName);
        }


        //My controllers 

        [HttpGet("/export/ITStockManagment/materielsPDR/csv")]
        [HttpGet("/export/ITStockManagment/materielsPDR/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportMaterielsPDRToCSV(string fileName = null)
        {
            return ToCSV((await service.GetMateriels()).Where(m => m.QuantityPDRStock!=0).Select( m => new
            {
                m.MaterielName,
                m.Type,
                m.SerialNumber,
                m.QuantityPDRStock,
                m.Warranty,
                

            }), fileName);
        }

        [HttpGet("/export/ITStockManagment/materielsPDR/excel")]
        [HttpGet("/export/ITStockManagment/materielsPDR/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportMaterielsPDRToExcel(string fileName = null)
        {
            return ToExcel((await service.GetMateriels()).Where(m => m.QuantityPDRStock != 0).Select(m => new
            {
                m.MaterielName,
                m.Type,
                m.SerialNumber,
                m.QuantityPDRStock,
                m.Warranty,
               

            }), fileName);
        }

        [HttpGet("/export/ITStockManagment/archived-requests/csv")]
        [HttpGet("/export/ITStockManagment/archived-requests/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportArchivedRequestsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery((await service.GetRequests()).Where(request => request.Offers != null && request.Offers.Any(offer => offer.Selected != null && offer.Selected == true && offer.DeliveryDate < DateTime.Today)), Request.Query, false), fileName);
        }

        [HttpGet("/export/ITStockManagment/archived-requests/excel")]
        [HttpGet("/export/ITStockManagment/archived-requests/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportArchivedRequestsToExcel(string fileName = null)
        {
            return ToCSV(ApplyQuery((await service.GetRequests()).Where(request => request.Offers != null && request.Offers.Any(offer => offer.Selected != null && offer.Selected == true && offer.DeliveryDate < DateTime.Today)), Request.Query, false), fileName);
        }


        [HttpGet("/export/ITStockManagment/materielsIT/csv")]
        [HttpGet("/export/ITStockManagment/materielsIT/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportMaterielsITToCSV(string fileName = null)
        {
            return ToCSV((await service.GetMateriels()).Where(m => m.QuantityITStock != 0).Select(m => new
            {
                m.MaterielName,
                m.Type,
                m.SerialNumber,
                m.QuantityITStock,
                m.Warranty,
                m.Repairing_Quantity,
                m.IrreparableQuantity
                

            }), fileName);
        }

        [HttpGet("/export/ITStockManagment/materielsIT/excel")]
        [HttpGet("/export/ITStockManagment/materielsIT/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportMaterielsITToExcel(string fileName = null)
        {
            return ToExcel((await service.GetMateriels()).Where(m => m.QuantityITStock != 0).Select(m => new
            {
                m.MaterielName,
                m.Type,
                m.SerialNumber,
                m.QuantityITStock,
                m.Warranty,
                m.Repairing_Quantity,
                m.IrreparableQuantity


            }), fileName);
        }

        // ─── Asset Lifecycle Module exports ───────────────────────────────────────

        [HttpGet("/export/ITStockManagment/maintenance/csv")]
        [HttpGet("/export/ITStockManagment/maintenance/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportMaintenanceToCSV(string fileName = null)
        {
            var tickets = await context.MaintenanceTickets
                .Include(t => t.Materiel)
                .Include(t => t.ReportedBy)
                .OrderByDescending(t => t.ReportedAt)
                .Select(t => new
                {
                    TicketId          = t.Id,
                    AssetName         = t.Materiel.MaterielName,
                    SerialNumber      = t.Materiel.SerialNumber,
                    ProblemDescription= t.ProblemDescription,
                    Status            = t.Status,
                    ReportedBy        = t.ReportedBy != null ? t.ReportedBy.FullName : "",
                    ReportedAt        = t.ReportedAt,
                    ResolvedAt        = t.ResolvedAt,
                    Resolution        = t.Resolution,
                    Cost              = t.Cost
                })
                .ToListAsync();
            return ToCSV(tickets.AsQueryable(), fileName);
        }

        [HttpGet("/export/ITStockManagment/maintenance/excel")]
        [HttpGet("/export/ITStockManagment/maintenance/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportMaintenanceToExcel(string fileName = null)
        {
            var tickets = await context.MaintenanceTickets
                .Include(t => t.Materiel)
                .Include(t => t.ReportedBy)
                .OrderByDescending(t => t.ReportedAt)
                .Select(t => new
                {
                    TicketId          = t.Id,
                    AssetName         = t.Materiel.MaterielName,
                    SerialNumber      = t.Materiel.SerialNumber,
                    ProblemDescription= t.ProblemDescription,
                    Status            = t.Status,
                    ReportedBy        = t.ReportedBy != null ? t.ReportedBy.FullName : "",
                    ReportedAt        = t.ReportedAt,
                    ResolvedAt        = t.ResolvedAt,
                    Resolution        = t.Resolution,
                    Cost              = t.Cost
                })
                .ToListAsync();
            return ToExcel(tickets.AsQueryable(), fileName);
        }

        [HttpGet("/export/ITStockManagment/lifecycle/csv")]
        [HttpGet("/export/ITStockManagment/lifecycle/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportLifecycleToCSV(string fileName = null)
        {
            var records = await context.AssetLifecycleRecords
                .Include(r => r.Materiel)
                .OrderByDescending(r => r.StartDate)
                .Select(r => new
                {
                    AssetName    = r.Materiel.MaterielName,
                    SerialNumber = r.Materiel.SerialNumber,
                    Stage        = r.Stage,
                    StartDate    = r.StartDate,
                    EndDate      = r.EndDate,
                    Notes        = r.Notes
                })
                .ToListAsync();
            return ToCSV(records.AsQueryable(), fileName);
        }

        [HttpGet("/export/ITStockManagment/lifecycle/excel")]
        [HttpGet("/export/ITStockManagment/lifecycle/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportLifecycleToExcel(string fileName = null)
        {
            var records = await context.AssetLifecycleRecords
                .Include(r => r.Materiel)
                .OrderByDescending(r => r.StartDate)
                .Select(r => new
                {
                    AssetName    = r.Materiel.MaterielName,
                    SerialNumber = r.Materiel.SerialNumber,
                    Stage        = r.Stage,
                    StartDate    = r.StartDate,
                    EndDate      = r.EndDate,
                    Notes        = r.Notes
                })
                .ToListAsync();
            return ToExcel(records.AsQueryable(), fileName);
        }

        [HttpGet("/export/ITStockManagment/predictions/csv")]
        [HttpGet("/export/ITStockManagment/predictions/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportPredictionsToCSV(string fileName = null)
        {
            var predictions = await context.AssetPredictions
                .Include(p => p.Materiel)
                .OrderBy(p => p.HealthScore)
                .Select(p => new
                {
                    AssetName                = p.Materiel.MaterielName,
                    SerialNumber             = p.Materiel.SerialNumber,
                    HealthScore              = p.HealthScore,
                    HealthStatus             = p.HealthStatus,
                    CalculatedAt             = p.CalculatedAt,
                    PredictedFailureDate     = p.PredictedFailureDate,
                    RecommendedReplacement   = p.RecommendedReplacementDate,
                    EstimatedCostEUR         = p.EstimatedReplacementCost,
                    Reason                   = p.RecommendationReason
                })
                .ToListAsync();
            return ToCSV(predictions.AsQueryable(), fileName);
        }

        [HttpGet("/export/ITStockManagment/predictions/excel")]
        [HttpGet("/export/ITStockManagment/predictions/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportPredictionsToExcel(string fileName = null)
        {
            var predictions = await context.AssetPredictions
                .Include(p => p.Materiel)
                .OrderBy(p => p.HealthScore)
                .Select(p => new
                {
                    AssetName                = p.Materiel.MaterielName,
                    SerialNumber             = p.Materiel.SerialNumber,
                    HealthScore              = p.HealthScore,
                    HealthStatus             = p.HealthStatus,
                    CalculatedAt             = p.CalculatedAt,
                    PredictedFailureDate     = p.PredictedFailureDate,
                    RecommendedReplacement   = p.RecommendedReplacementDate,
                    EstimatedCostEUR         = p.EstimatedReplacementCost,
                    Reason                   = p.RecommendationReason
                })
                .ToListAsync();
            return ToExcel(predictions.AsQueryable(), fileName);
        }
    }
}
