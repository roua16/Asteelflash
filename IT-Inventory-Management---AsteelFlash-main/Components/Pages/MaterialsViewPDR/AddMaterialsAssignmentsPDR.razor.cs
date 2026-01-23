using ITStockM.Models.ITStockManagment;
using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsViewPDR
{
    public partial class AddMaterialsAssignmentsPDR
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }


       
        protected override async Task OnInitializedAsync()
        {
            

            assignment = new Assignment();

           
            materiels = (await ITStockManagmentService.GetMateriels()).Where(m => m.QuantityPDRStock != 0).OrderBy(m => m.Warranty).ToList();



            listMaterials = new List<MatList>();
            previouslySelectedMateriels = new List<Materiel?>();
            myDropDowns = new List<RadzenDropDownDataGrid<Materiel>>();

            listMaterials.Add(new MatList { Qtee = 1 });
            previouslySelectedMateriels.Add(null);
            myDropDowns.Add(new RadzenDropDownDataGrid<Materiel>());
            
            assignment.AssignedBy = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value.Id;
            assignment.AssignedTo = 6; 

        }




        protected List<MatList> listMaterials;
        protected bool errorVisible;
        protected string errorMsg = "";

        protected Assignment assignment;



        protected List<Materiel> materiels;
        private List<RadzenDropDownDataGrid<Materiel>> myDropDowns;
        protected List<Materiel?> previouslySelectedMateriels;
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
                await ITStockManagmentService.CreateAssignment(assignment);

                foreach (var mat in listMaterials)
                {
                    var newAssignmentMaterial = new ITStockM.Models.ITStockManagment.AssignmentMateriel
                    {
                        MaterielId = mat.Materiel.Id,
                        AssignmentId = assignment.Id,
                        Qte = mat.Qtee

                    };
                    mat.Materiel.QuantityPDRStock = mat.Materiel.QuantityPDRStock - mat.Qtee;
                    mat.Materiel.QuantityITStock = mat.Materiel.QuantityITStock + mat.Qtee;
                    await ITStockManagmentService.UpdateMateriel(mat.Materiel.Id,mat.Materiel);
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

    }

    public class Option
    {
        public string Label { get; set; }
        public bool Value { get; set; }
    }

    public class MatList
    {
        public ITStockM.Models.ITStockManagment.Materiel Materiel { get; set; }
        public int Qtee { get; set; }


    }
}