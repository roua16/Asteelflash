using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace ITStockM.Services.Export
{
    /// <summary>
    /// Service for handling data exports to Excel and CSV formats
    /// </summary>
    public class ExportService : IExportService
    {
        private readonly NavigationManager _navigationManager;

        public ExportService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public Task ExportToExcel(string endpoint, Query query = null, string fileName = null)
        {
            var encodedFileName = !string.IsNullOrEmpty(fileName) 
                ? UrlEncoder.Default.Encode(fileName) 
                : "Export";

            var url = query != null
                ? query.ToUrl($"{endpoint}/excel(fileName='{encodedFileName}')")
                : $"{endpoint}/excel(fileName='{encodedFileName}')";

            _navigationManager.NavigateTo(url, true);
            return Task.CompletedTask;
        }

        public Task ExportToCSV(string endpoint, Query query = null, string fileName = null)
        {
            var encodedFileName = !string.IsNullOrEmpty(fileName) 
                ? UrlEncoder.Default.Encode(fileName) 
                : "Export";

            var url = query != null
                ? query.ToUrl($"{endpoint}/csv(fileName='{encodedFileName}')")
                : $"{endpoint}/csv(fileName='{encodedFileName}')";

            _navigationManager.NavigateTo(url, true);
            return Task.CompletedTask;
        }
    }
}
