
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services.AssignmentMateriels;
using ITStockM.Services.Assignments;
using ITStockM.Services.Materiels;


namespace ITStockM.Components.Pages.CrudPages
{
    public partial class AddAssignmentMateriel
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IAssignmentMaterielService AssignmentMaterielService { get; set; } = default!;

        [Inject]
        public IMaterielService MaterielService { get; set; } = default!;

        [Inject]
        public IAssignmentService AssignmentService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            assignmentMateriel = new Domain.Entities.AssignmentMateriel();

            materielsForMaterielId = (await MaterielService.GetMateriels()).ToList();

            assignmentsForAssignmentId = await AssignmentService.GetAssignments(new Query());
        }
        protected bool errorVisible;
        protected Domain.Entities.AssignmentMateriel assignmentMateriel = new();

        protected IEnumerable<Domain.Entities.Materiel> materielsForMaterielId = new List<Domain.Entities.Materiel>();

        protected IEnumerable<Domain.Entities.Assignment> assignmentsForAssignmentId = new List<Domain.Entities.Assignment>();

        protected async Task FormSubmit()
        {
            try
            {
                await AssignmentMaterielService.CreateAssignmentMateriel(assignmentMateriel);
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