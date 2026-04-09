using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace ITStockM.Components.Pages.MaterialsAssignments
{
    public partial class MaterialsAssignmentsDetails
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; } = default!;

        [Parameter]
        public Models.ITStockManagment.AssignmentMateriel AssignmentMateriel { get; set; } = new();

        protected async Task ReturnMateriel()
        {
            if (AssignmentMateriel.Materiel is null || AssignmentMateriel.Assignment is null)
            {
                return;
            }

            var options = new DialogOptions
            {
                Style = "min-width: 600px;",
                CssClass = "dialog-animation",
                CloseDialogOnOverlayClick = true,
                Resizable = true,
                Draggable = true,
                CloseDialogOnEsc = true
            };

            var result = await DialogService.OpenAsync<ConfirmMaterialReturn>(
                "",
                new Dictionary<string, object>
                {
                    { "HasSN", string.IsNullOrEmpty(AssignmentMateriel.Materiel.SerialNumber) },
                    { "MaxQte", AssignmentMateriel.Qte },
                    { "MaterialName", AssignmentMateriel.Materiel.MaterielName }
                },
                options);

            if (result is not IDictionary<string, object> values)
            {
                return;
            }

            var selectedOption = values.TryGetValue("selectedOption", out var selectedOptionObj)
                ? selectedOptionObj?.ToString()
                : null;
            var quantity = values.TryGetValue("qte", out var quantityObj) && int.TryParse(quantityObj?.ToString(), out var parsedQuantity)
                ? parsedQuantity
                : 0;

            if (quantity <= 0)
            {
                return;
            }

            var mat = AssignmentMateriel.Materiel;
            if (selectedOption == "Good Conditions")
            {
                mat.QuantityITStock += quantity;
            }
            else if (selectedOption == "Need to be repaired")
            {
                mat.Repairing_Quantity += quantity;
            }
            else
            {
                mat.IrreparableQuantity += quantity;
            }

            await ITStockManagmentService.UpdateMateriel(mat.Id, mat);

            var assignment = AssignmentMateriel.Assignment;
            if (values.TryGetValue("description", out var descriptionObj))
            {
                var description = descriptionObj?.ToString();
                if (!string.IsNullOrWhiteSpace(description))
                {
                    assignment.Descipriton = string.Concat(assignment.Descipriton ?? string.Empty, description);
                }
            }

            AssignmentMateriel.Qte -= quantity;

            if (AssignmentMateriel.Qte == 0)
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