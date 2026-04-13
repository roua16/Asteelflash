using ITStockM.Domain.Entities;
using ITStockM.Services;
using ITStockM.Services.Materiels;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsView
{
    public partial class ConfirmConditionChange
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;
        [Inject]
        protected NotificationService NotificationService { get; set; } = default!;
        [Inject]
        public IMaterielService MaterielService { get; set; } = default!;

        [Parameter]
        public int MaxQte { get; set; }

        [Parameter]
        public bool HasSN { get; set; }

        [Parameter]
        public List<Materiel> materiels { get; set; } = new();

        [Parameter]
        public string CurrentCondition { get; set; } = string.Empty;


        protected int Qte = 1;

        protected List<string> options = new();

        protected string selectedOption = string.Empty;
        private List<RadzenDropDownDataGrid<Materiel>> myDropDowns = new();
        protected List<Materiel?> previouslySelectedMateriels = new();

       
        protected List<MatList> listMaterials = new();


        protected override async Task OnInitializedAsync()
        {
            if (CurrentCondition == "IT")
            {
                options = new List<string> {  "Need to be repaired", "Unrepairable" };
            }
            else
            {
                options = new List<string> { "Repaired",  "Unrepairable" };
            }

            selectedOption = options.FirstOrDefault() ?? string.Empty;

                listMaterials = new List<MatList>();
            previouslySelectedMateriels = new List<Materiel?>();
            listMaterials.Add(new MatList {});
            previouslySelectedMateriels.Add(null);
            myDropDowns = new List<RadzenDropDownDataGrid<Materiel>>();
            myDropDowns.Add(new RadzenDropDownDataGrid<Materiel>());
        }

        private async Task deleteMaterial(int i)
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

        protected async Task RemoveMatOnSelect(object e, int i)
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
            if (listMaterials.Count >= materiels.Count)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                   
                    Detail = "No materials left",
                    Duration = 4000
                });
                return;
            }
            else
            {
                listMaterials.Add(new MatList { });
                previouslySelectedMateriels.Add(null);
                myDropDowns.Add(new RadzenDropDownDataGrid<Materiel>());
            }
        }

        protected async Task submit()
        {
            if (HasSN)
            {
               
                foreach (var item in listMaterials)
                {



                    if (selectedOption == "Need to be repaired")
                    {
                       
                        item.Materiel.QuantityITStock = item.Materiel.QuantityITStock - Qte;
                        item.Materiel.Repairing_Quantity = item.Materiel.Repairing_Quantity + Qte;
                        await MaterielService.UpdateMateriel(item.Materiel.Id,item.Materiel);

                    }
                    else if (selectedOption == "Repaired")
                    {
                        
                        item.Materiel.Repairing_Quantity = item.Materiel.Repairing_Quantity - Qte;
                        item.Materiel.QuantityITStock = item.Materiel.QuantityITStock + Qte;
                        await MaterielService.UpdateMateriel(item.Materiel.Id, item.Materiel);
                    }
                    else
                    {
                        if (CurrentCondition == "IT")
                        {
                            
                            item.Materiel.QuantityITStock = item.Materiel.QuantityITStock - Qte;
                            item.Materiel.IrreparableQuantity = item.Materiel.IrreparableQuantity + Qte;
                            await MaterielService.UpdateMateriel(item.Materiel.Id, item.Materiel);
                        }
                        else
                        {
                            
                            item.Materiel.Repairing_Quantity = item.Materiel.Repairing_Quantity - Qte;
                            item.Materiel.IrreparableQuantity = item.Materiel.IrreparableQuantity + Qte;
                            await MaterielService.UpdateMateriel(item.Materiel.Id, item.Materiel);
                        }
                    }

                }

                

            }
            else
            {
                var mat = materiels.FirstOrDefault();

                

                if (selectedOption == "Need to be repaired")
                {
                   
                    mat.QuantityITStock = mat.QuantityITStock - Qte;
                    mat.Repairing_Quantity = mat.Repairing_Quantity + Qte;
                    await MaterielService.UpdateMateriel(mat.Id, mat);
                }
                else if (selectedOption == "Repaired")
                {
                    
                    mat.Repairing_Quantity = mat.Repairing_Quantity - Qte;
                    mat.QuantityITStock = mat.QuantityITStock + Qte;
                    await MaterielService.UpdateMateriel(mat.Id, mat);
                }
                else
                {
                    if (CurrentCondition == "IT")
                    {
                        
                        mat.QuantityITStock = mat.QuantityITStock - Qte;
                        mat.IrreparableQuantity = mat.IrreparableQuantity + Qte;
                        await MaterielService.UpdateMateriel(mat.Id, mat);
                    }
                    else
                    {
                  
                        mat.Repairing_Quantity = mat.Repairing_Quantity - Qte;
                        mat.IrreparableQuantity = mat.IrreparableQuantity + Qte;
                        await MaterielService.UpdateMateriel(mat.Id, mat);
                    }
                }
               
            }
            DialogService.Close(true);
        }

    }
    public class MatList
    {
        public Materiel Materiel { get; set; } = new();
        


    }
}
