
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;


namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class AddAssignmentMateriel
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            assignmentMateriel = new Models.ITStockManagment.AssignmentMateriel();

            materielsForMaterielId = await ITStockManagmentService.GetMateriels();

            assignmentsForAssignmentId = await ITStockManagmentService.GetAssignments();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.AssignmentMateriel assignmentMateriel;

        protected IEnumerable<Models.ITStockManagment.Materiel> materielsForMaterielId;

        protected IEnumerable<Models.ITStockManagment.Assignment> assignmentsForAssignmentId;

        protected async Task FormSubmit()
        {
            try
            {
                await ITStockManagmentService.CreateAssignmentMateriel(assignmentMateriel);
                DialogService.Close(assignmentMateriel);
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