using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsAssignments
{
    public partial class MissionInterface
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }


        protected IEnumerable<Models.ITStockManagment.AssignmentMateriel> assignmentMateriels;

        protected RadzenDataGrid<Models.ITStockManagment.AssignmentMateriel> grid0;

        protected override async Task OnInitializedAsync()
        {
            assignmentMateriels = await ITStockManagmentService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment" }); // change in the service to include employee

            assignmentMateriels = assignmentMateriels.Where(assm => assm.Assignment.OnMission == true && assm.Assignment.RestoreDate == null && assm.Qte != 0 );



        }

        protected async Task MissionEnd(Models.ITStockManagment.AssignmentMateriel assignmentMateriel)
        {

            var options = new DialogOptions
            {
                Style = "min-width: 600px;", 
                CssClass = "dialog-animation",
                CloseDialogOnOverlayClick = true,
                Resizable = true,
                Draggable = true,
                CloseDialogOnEsc = true
            };

            var result = await DialogService.OpenAsync<ConfirmMaterialReturn>("", new Dictionary<string, object> { { "HasSN", assignmentMateriel.Materiel.SerialNumber == null }, { "MaxQte", assignmentMateriel.Qte }, { "MaterialName", assignmentMateriel.Materiel.MaterielName }}, options);

            if (result != null)
            {
                var mat = assignmentMateriel.Materiel;
                if (result["selectedOption"] == "Good Conditions")
                {
                    mat.QuantityITStock = mat.QuantityITStock + result["qte"];
                }
                else if (result["selectedOption"] == "Need to be repaired")
                {
                    mat.Repairing_Quantity = mat.Repairing_Quantity + result["qte"];
                }
                else
                {
                    mat.IrreparableQuantity = mat.IrreparableQuantity + result["qte"];
                }



                await ITStockManagmentService.UpdateMateriel(mat.Id, mat);

                var assignment = assignmentMateriel.Assignment;
                assignment.Descipriton = assignment.Descipriton + result["description"];

                assignmentMateriel.Qte = assignmentMateriel.Qte - result["qte"];

                if (assignmentMateriel.Qte == 0 && assignment.AssignmentMateriels.All(assm => assm.Qte == 0))
                {
                    assignment.RestoreDate = DateTime.Now;
                }




                await ITStockManagmentService.UpdateAssignment(assignment.Id, assignment);


                await ITStockManagmentService.UpdateAssignmentMateriel(assignmentMateriel.MaterielId, assignmentMateriel.AssignmentId, assignmentMateriel);

            }


            await OnInitializedAsync();
        }
    }
}
