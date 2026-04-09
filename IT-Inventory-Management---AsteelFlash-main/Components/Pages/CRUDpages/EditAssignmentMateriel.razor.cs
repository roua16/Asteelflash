
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services.AssignmentMateriels;
using ITStockM.Services.Assignments;
using ITStockM.Services.Materiels;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class EditAssignmentMateriel
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IAssignmentMaterielService AssignmentMaterielService { get; set; } = default!;

        [Inject]
        public IMaterielService MaterielService { get; set; } = default!;

        [Inject]
        public IAssignmentService AssignmentService { get; set; } = default!;

        [Parameter]
        public int MaterielId { get; set; }

        [Parameter]
        public int AssignmentId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            assignmentMateriel = await AssignmentMaterielService.GetAssignmentMaterielByMaterielIdAndAssignmentId(MaterielId, AssignmentId) ?? new Models.ITStockManagment.AssignmentMateriel();

            materielsForMaterielId = (await MaterielService.GetMateriels()).ToList();

            assignmentsForAssignmentId = await AssignmentService.GetAssignments(new Query());
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.AssignmentMateriel assignmentMateriel = new();

        protected IEnumerable<Models.ITStockManagment.Materiel> materielsForMaterielId = new List<Models.ITStockManagment.Materiel>();

        protected IEnumerable<Models.ITStockManagment.Assignment> assignmentsForAssignmentId = new List<Models.ITStockManagment.Assignment>();

        protected async Task FormSubmit()
        {
            try
            {
                await AssignmentMaterielService.UpdateAssignmentMateriel(MaterielId, AssignmentId, assignmentMateriel);
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