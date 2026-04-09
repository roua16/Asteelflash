using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;
using ITStockM.Services.Projects;


namespace ITStockM.Components.Pages.CrudPages
{
    public partial class AddProject
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;
        [Inject]
        public IProjectService ProjectService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            project = new Models.ITStockManagment.Project();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.Project project = new();

        protected async Task FormSubmit()
        {
            try
            {
                await ProjectService.CreateProject(project);
                DialogService.Close(project);
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
    }
}