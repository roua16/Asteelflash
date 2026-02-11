using ITStockM.Models.ITStockManagment;
using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsAssignments
{
    public partial class AddMaterialsAssignments
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }



        List<Option> options = new List<Option>
    {
        new Option { Label = "Mission", Value = true },
        new Option { Label = "Assignment", Value = false }
    };

        protected int personOrProject = 1;
        protected override async Task OnInitializedAsync()
        {
            assignment = new Assignment();

            projects = await ITStockManagmentService.GetProjectsList();
            
            employeesForAssignedBy = await ITStockManagmentService.GetEmployeesList();

            materiels = (await ITStockManagmentService.GetMateriels()).Where(m =>  m.QuantityITStock != 0).OrderBy(m => m.Warranty).ToList(); 

            
            listMaterials = new List<MatList>();
            previouslySelectedMateriels = new List<Materiel?> ();
            listMaterials.Add(new MatList { Qtee = 1  });
            previouslySelectedMateriels.Add(null);
            myDropDowns = new List<RadzenDropDownDataGrid<Materiel>>();
            myDropDowns.Add(new RadzenDropDownDataGrid<Materiel>());

            assignment.AssignedBy = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value.Id;

        }




        protected List<MatList> listMaterials = new();
        protected bool errorVisible;
        protected string errorMsg = "";

        protected Assignment assignment;

        protected IEnumerable<Employee> employeesForAssignedBy = new List<Employee>();

        protected IEnumerable<Project> projects = new List<Project>();

        protected List<Materiel> materiels = new();

        private List<RadzenDropDownDataGrid<Materiel>> myDropDowns = new();
        protected List<Materiel?> previouslySelectedMateriels = new();


        protected void addMaterial()
        {
            listMaterials.Add(new MatList { Qtee = 1 });
            previouslySelectedMateriels.Add(null);
            myDropDowns.Add(new RadzenDropDownDataGrid<Materiel>());
        }
        protected async Task FormSubmit()
        {
  
            try
            {

                assignment.Date = DateTime.Now;
                var assignmentCreated = await ITStockManagmentService.CreateAssignment(assignment);

                foreach (var mat in listMaterials)
                {
 
                    mat.Materiel.QuantityITStock = mat.Materiel.QuantityITStock - mat.Qtee;

                    var newAssignmentMaterial = new AssignmentMateriel
                    {
                        MaterielId = mat.Materiel.Id,
                        Materiel = mat.Materiel,
                        Qte = mat.Qtee,
                        AssignmentId = assignmentCreated.Id

                    };

                    await ITStockManagmentService.UpdateMateriel(newAssignmentMaterial.MaterielId, newAssignmentMaterial.Materiel);
                    await ITStockManagmentService.CreateAssignmentMateriel(newAssignmentMaterial);
                   

                }
                DialogService.Close(true);

            }
            catch (Exception ex)
            {
                errorVisible = true;
                
            }

        }

        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close();
        }
       
        private async void deleteMaterial(int i)
        {
            listMaterials.RemoveAt(i);
            if (previouslySelectedMateriels[i] != null)
            {
                materiels.Add(previouslySelectedMateriels[i]);
            }
           
            myDropDowns.RemoveAt(i);
            for (int j = 0; j < myDropDowns.Count; j++)
            {
                await myDropDowns[j].Reload();
            }
        }

        protected async void RemoveMatOnSelect(object e, int i)
        {
            var newSelection = e as Materiel;

            if (newSelection != null)
            {
               
                var materialToRemove = materiels.FirstOrDefault(m => m.Id == newSelection.Id);
                if (materialToRemove != null)
                {
                    materiels.Remove(materialToRemove);
                }

                if (previouslySelectedMateriels[i] != null)
                {
                    if (!materiels.Any(m => m.Id == previouslySelectedMateriels[i].Id))
                    {
                        materiels.Add(previouslySelectedMateriels[i]);
                    }
                }

                previouslySelectedMateriels[i] = newSelection;
            }
            else 
            {
                if (previouslySelectedMateriels[i] != null)
                {
                    if (!materiels.Any(m => m.Id == previouslySelectedMateriels[i].Id))
                    {
                        materiels.Add(previouslySelectedMateriels[i]);
                    }
                    previouslySelectedMateriels[i] = null;
                }
            }
            for (int j = 0; j < myDropDowns.Count; j++)
            {
                await myDropDowns[j].Reload();
            }
            await InvokeAsync(StateHasChanged);
        }
    }

    public class Option
    {
        public string Label { get; set; }
        public bool Value { get; set; }
    }

    public class MatList
    {
        public Materiel Materiel { get; set; }
        public int Qtee { get; set; }


    }
}