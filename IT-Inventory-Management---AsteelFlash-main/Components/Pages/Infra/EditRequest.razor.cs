using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;

namespace ITStockM.Components.Pages.Infra
{
    public partial class EditRequest
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; } = default!;

        [Parameter]
        public int Id { get; set; }

        protected List<string> requestOptions = new List<string>() { "Update project", "Existing Project" };
        protected List<string> materialTypes = new List<string>() { "HardWare", "SoftWare", "Mosue", "KeyBoard", "Laptop", "Mini Pc", "Backpack", "Headphone", "Monitor" };
        protected List<string> status = new List<string>() { "Normal", "Urgent", "Critical" };

        protected bool errorVisible;
        protected Models.ITStockManagment.Request request = new();

        protected List<string> projectNames = new();
        protected override async Task OnInitializedAsync()
        {
            projectNames = new List<string>();
            request = await ITStockManagmentService.GetRequestById(Id);

            projectNames = (await ITStockManagmentService.GetProjectsList()).Select(p => p.ProjectName).ToList();

            if (request.File != null)
            {
                await JSRuntime.InvokeVoidAsync("downloadFile",
                    request.FileName,
                    request.File);
            }
        }

        private async Task OnFileUpload(Radzen.UploadChangeEventArgs args)
        {



            var file = args.Files.FirstOrDefault();

            if (file != null)
            {
                using (var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024))
                {
                    var buffer = new byte[stream.Length];
                    await stream.ReadExactlyAsync(buffer, 0, buffer.Length);
                    request.File = buffer;
                    request.FileExtension = System.IO.Path.GetExtension(file.Name);
                    request.FileName = file.Name;
                }
            }
        }

        protected async Task FormSubmit()
        {
            try
            {
                await ITStockManagmentService.UpdateRequest(Id, request);
                DialogService.Close(request);
            }
            catch (Exception ex)
            {
                errorVisible = true;
            }
        }

        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close(null);
        }

        protected async Task DownloadFile()
        {
            if (request.File != null)
            {
                await JSRuntime.InvokeVoidAsync("downloadFile",
                    request.FileName,
                    request.File);
            }
        }
    }
}