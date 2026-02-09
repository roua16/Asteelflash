using Radzen;

namespace ITStockM.Services.Export
{
    /// <summary>
    /// Service interface for exporting data to various formats
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Triggers a navigation to export data to Excel
        /// </summary>
        Task ExportToExcel(string endpoint, Query query = null, string fileName = null);

        /// <summary>
        /// Triggers a navigation to export data to CSV
        /// </summary>
        Task ExportToCSV(string endpoint, Query query = null, string fileName = null);
    }
}
