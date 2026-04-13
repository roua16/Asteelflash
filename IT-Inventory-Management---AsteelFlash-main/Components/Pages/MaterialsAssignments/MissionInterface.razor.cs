using ITStockM.Models.Constants;
using ITStockM.Services.AssignmentMateriels;
using ITStockM.Services.Assignments;
using ITStockM.Services.Employees;
using ITStockM.Services.Materiels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using ITStockM.Services.Interfaces;

namespace ITStockM.Components.Pages.MaterialsAssignments
{
    public partial class MissionInterface
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public IAssignmentMaterielService AssignmentMaterielService { get; set; }

        [Inject]
        public IAssignmentService AssignmentService { get; set; }

        [Inject]
        public IMaterielService MaterielService { get; set; }

        [Inject]
        public IEmployeeService EmployeeService { get; set; }

        [Inject]
        protected IOperationNotificationService OperationNotificationService { get; set; }


        protected IEnumerable<Domain.Entities.AssignmentMateriel> assignmentMateriels;

        protected RadzenDataGrid<Domain.Entities.AssignmentMateriel> grid0;

        protected override async Task OnInitializedAsync()
        {
            assignmentMateriels = await AssignmentMaterielService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment" }); // change in the service to include employee

            assignmentMateriels = assignmentMateriels.Where(assm => assm.Assignment.OnMission == true && assm.Assignment.RestoreDate == null && assm.Qte != 0 );



        }

        protected async Task MissionEnd(Domain.Entities.AssignmentMateriel assignmentMateriel)
        {

            var originalQte = assignmentMateriel.Qte;

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

                await MaterielService.UpdateMateriel(mat.Id, mat);

                var assignment = assignmentMateriel.Assignment;
                assignment.Descipriton = assignment.Descipriton + result["description"];

                assignmentMateriel.Qte = assignmentMateriel.Qte - result["qte"];

                if (assignmentMateriel.Qte == 0 && assignment.AssignmentMateriels.All(assm => assm.Qte == 0))
                {
                    assignment.RestoreDate = DateTime.Now;
                }


                await AssignmentService.UpdateAssignment(assignment.Id, assignment);


                await AssignmentMaterielService.UpdateAssignmentMateriel(assignmentMateriel.MaterielId, assignmentMateriel.AssignmentId, assignmentMateriel);

                // Prepare and send notifications if there were issues detected (missing/damaged)
                var selectedOptLocal = result.ContainsKey("selectedOption") ? (string)result["selectedOption"] : "Good Conditions";
                var returnedQteLocal = result.ContainsKey("qte") ? Convert.ToInt32(result["qte"]) : 0;
                var descriptionLocal = result.ContainsKey("description") ? result["description"]?.ToString() : string.Empty;

                // Check for other material returned (not part of assignment)
                var otherName = result.ContainsKey("otherName") ? (result["otherName"]?.ToString() ?? string.Empty) : string.Empty;
                var otherQty = result.ContainsKey("otherQty") ? Convert.ToInt32(result["otherQty"]) : 0;

                if (!string.IsNullOrWhiteSpace(otherName) && otherQty > 0)
                {
                    await RegisterReturnedMaterial(otherName.Trim(), otherQty, selectedOptLocal);
                }

                await HandleReturnIssues(assignment, assignmentMateriel, originalQte, selectedOptLocal, returnedQteLocal, descriptionLocal);

            }


            await OnInitializedAsync();
        }

        private async Task HandleReturnIssues(Domain.Entities.Assignment assignment, Domain.Entities.AssignmentMateriel assignmentMateriel, int originalQte, string selectedOption, int returnedQte, string description)
        {
            var issues = new List<string>();

            if (returnedQte < originalQte)
            {
                var missed = originalQte - returnedQte;
                issues.Add($"Missing {missed} x {assignmentMateriel.Materiel.MaterielName} (expected {originalQte}, returned {returnedQte})");
            }

            if (selectedOption != "Good Conditions")
            {
                issues.Add($"Returned in non-good condition: {selectedOption} for {returnedQte} x {assignmentMateriel.Materiel.MaterielName}");
            }

            var remainTotal = assignment.AssignmentMateriels.Sum(assm => assm.Qte);
            if (remainTotal > 0)
            {
                issues.Add($"Remaining items not returned for assignment: {remainTotal}");
            }

            if (!issues.Any())
                return;

            var issueDetails = string.Join("\n", issues) + "\n\nNotes:\n" + description;

            // collect recipients: Admin + PDR + IT emails from personnel list
            var employees = await EmployeeService.GetEmployeesList();
            var recipients = employees.Where(e => e.Role == UserRoles.Admin || e.Role == UserRoles.PDR || e.Role == UserRoles.IT)
                .Select(e => e.Email)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .ToList();

            if (!recipients.Any())
            {
                // fallback to environment admin email
                var admin = Environment.GetEnvironmentVariable("SMTP_ADMIN_EMAIL") ?? "admin@asteelflash.com";
                recipients.Add(admin);
            }

            await OperationNotificationService.NotifyAssignmentReturnIssue(assignment, issueDetails, recipients);
        }

        private async Task RegisterReturnedMaterial(string materialName, int quantity, string condition)
        {
            var existing = await MaterielService.GetMaterielByName(materialName);

            if (existing != null)
            {
                if (condition == "Good Conditions")
                {
                    existing.QuantityITStock += quantity;
                }
                else if (condition == "Need to be repaired")
                {
                    existing.Repairing_Quantity += quantity;
                }
                else
                {
                    existing.IrreparableQuantity += quantity;
                }

                await MaterielService.UpdateMateriel(existing.Id, existing);
                return;
            }

            var newMateriel = new Domain.Entities.Materiel
            {
                MaterielName = materialName,
                Type = "Other",
                Warranty = DateTime.Today
            };

            if (condition == "Good Conditions")
            {
                newMateriel.QuantityITStock = quantity;
            }
            else if (condition == "Need to be repaired")
            {
                newMateriel.Repairing_Quantity = quantity;
            }
            else
            {
                newMateriel.IrreparableQuantity = quantity;
            }

            await MaterielService.CreateMateriel(newMateriel);
        }
    }
}
