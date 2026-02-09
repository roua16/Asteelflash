using ITStockM.Services;
using ITStockM.Models.Constants;
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
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Inject]
        protected IOperationNotificationService OperationNotificationService { get; set; }


        protected IEnumerable<Models.ITStockManagment.AssignmentMateriel> assignmentMateriels;

        protected RadzenDataGrid<Models.ITStockManagment.AssignmentMateriel> grid0;

        protected override async Task OnInitializedAsync()
        {
            assignmentMateriels = await ITStockManagmentService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment" }); // change in the service to include employee

            assignmentMateriels = assignmentMateriels.Where(assm => assm.Assignment.OnMission == true && assm.Assignment.RestoreDate == null && assm.Qte != 0 );



        }

        protected async Task MissionEnd(Models.ITStockManagment.AssignmentMateriel assignmentMateriel)
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

                // Prepare and send notifications if there were issues detected (missing/damaged)
                var selectedOptLocal = result.ContainsKey("selectedOption") ? (string)result["selectedOption"] : "Good Conditions";
                var returnedQteLocal = result.ContainsKey("qte") ? Convert.ToInt32(result["qte"]) : 0;
                var descriptionLocal = result.ContainsKey("description") ? result["description"]?.ToString() : string.Empty;

                // Check for other material returned (not part of assignment)
                var otherName = result.ContainsKey("otherName") ? (result["otherName"]?.ToString() ?? string.Empty) : string.Empty;
                var otherQty = result.ContainsKey("otherQty") ? Convert.ToInt32(result["otherQty"]) : 0;

                if (!string.IsNullOrWhiteSpace(otherName) && otherQty > 0)
                {
                    // register returned material in inventory (create if missing) and send notifications
                    _ = ITStockManagmentService.RegisterReturnedMaterial(otherName.Trim(), otherQty, selectedOptLocal);
                }

                await HandleReturnIssues(assignment, assignmentMateriel, originalQte, selectedOptLocal, returnedQteLocal, descriptionLocal);

            }


            await OnInitializedAsync();
        }

        private async Task HandleReturnIssues(Models.ITStockManagment.Assignment assignment, Models.ITStockManagment.AssignmentMateriel assignmentMateriel, int originalQte, string selectedOption, int returnedQte, string description)
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
            var employees = (await ITStockManagmentService.GetEmployees()).ToList();
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
    }
}
