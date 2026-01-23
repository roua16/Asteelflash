
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class EditAssignmentMateriel
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public int MaterielId { get; set; }

        [Parameter]
        public int AssignmentId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            assignmentMateriel = await ITStockManagmentService.GetAssignmentMaterielByMaterielIdAndAssignmentId(MaterielId, AssignmentId);

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
                await ITStockManagmentService.UpdateAssignmentMateriel(MaterielId, AssignmentId, assignmentMateriel);
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