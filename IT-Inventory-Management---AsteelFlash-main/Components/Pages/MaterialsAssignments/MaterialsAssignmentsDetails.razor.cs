using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace ITStockM.Components.Pages.MaterialsAssignments
{
    public partial class MaterialsAssignmentsDetails
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public Models.ITStockManagment.AssignmentMateriel AssignmentMateriel { get; set; } 

        protected async Task  ReturnMateriel()
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

            var result = await DialogService.OpenAsync<ConfirmMaterialReturn>("", new Dictionary<string, object> { {"HasSN",AssignmentMateriel.Materiel.SerialNumber == null },{ "MaxQte", AssignmentMateriel.Qte }, { "MaterialName", AssignmentMateriel.Materiel.MaterielName } }, options);

            if (result != null)
            {
                


                var mat = AssignmentMateriel.Materiel;
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

                var assignment = AssignmentMateriel.Assignment;
                assignment.Descipriton = assignment.Descipriton + result["description"];
                
                AssignmentMateriel.Qte = AssignmentMateriel.Qte - result["qte"];
                
                

                
                if(AssignmentMateriel.Qte == 0)
                {
                    assignment.RestoreDate = DateTime.Now;
                    DialogService.Close(true);
                }
                else
                {
                    DialogService.Close(false);
                }

                await ITStockManagmentService.UpdateAssignment(assignment.Id, assignment);


                await ITStockManagmentService.UpdateAssignmentMateriel(AssignmentMateriel.MaterielId, AssignmentMateriel.AssignmentId, AssignmentMateriel);

            }
            


        }

    }
}