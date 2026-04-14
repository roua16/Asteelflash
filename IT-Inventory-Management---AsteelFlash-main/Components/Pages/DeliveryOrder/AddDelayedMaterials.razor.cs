
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using ITStockM.Models.ViewModels;
using ITStockM.Services.DeliveryOrderMateriels;
using ITStockM.Services.DeliveryOrders;
using ITStockM.Services.Materiels;


namespace ITStockM.Components.Pages.DeliveryOrder
{
    public partial class AddDelayedMaterials
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; } = default!;

        

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderService DeliveryOrderService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderMaterielService DeliveryOrderMaterielService { get; set; } = default!;

        [Inject]
        public IMaterielService MaterielService { get; set; } = default!;

        protected List<Domain.Entities.DeliveryOrder> DeliveryOrders { get; set; } = new();

        protected Domain.Entities.DeliveryOrder SelectedDeliveryOrders { get; set; } = new();

        protected List<MaterielViewModel> MaterielsList = new List<MaterielViewModel>();
        private IEnumerable<string> MaterialSuggestions { get; set; } = Enumerable.Empty<string>();
        protected List<string> options = new List<string> { "HardWare", "SoftWare", "Mouse", "KeyBoard", "Laptop", "Mini Pc", "Backpack", "Headphone", "Monitor", "Network device", "Printer", "Consumables" };

        private RadzenTemplateForm<Domain.Entities.DeliveryOrder> form = default!;
        protected bool changeSR = true;

        protected override async Task OnInitializedAsync()
        {
            DeliveryOrders = (await DeliveryOrderService.GetDeliveryOrdersList()).Where(dlo => dlo.HasDelayedM).ToList();
            SelectedDeliveryOrders = DeliveryOrders.FirstOrDefault() ?? new Domain.Entities.DeliveryOrder();

            //---
            MaterielsList = new List<MaterielViewModel>
                            {
                                new MaterielViewModel
                                {
                                    Materiel = new Domain.Entities.Materiel { MaterielName = "",  QuantityPDRStock = 1, QuantityITStock = 0 , IrreparableQuantity =0, Repairing_Quantity =0},
                                    HaveSr = false,
                                    SRList = new List<SerialNumber>(),
                                    Year = 0,
                                    Month = 0
                                }
                            };

            var mats = await MaterielService.GetMateriels();

            MaterialSuggestions = mats.Select(m => m.MaterielName).Distinct();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("preventEnterKeyFormSubmission");
            }
        }

      
        protected void AddMateriel()
        {

            var newMateriel = new MaterielViewModel
            {
                Materiel = new Domain.Entities.Materiel { MaterielName = "", QuantityPDRStock = 1, QuantityITStock = 0, IrreparableQuantity = 0, Repairing_Quantity = 0 },
                HaveSr = false,
                SRList = new List<SerialNumber>(),
                Year = 0,
                Month = 0
            };

            MaterielsList.Add(newMateriel);


        }
        protected void RemoveMateriel(int i)
        {
            MaterielsList.RemoveAt(i);


        }

        protected void TypeSRChecker(int i)
        {

            if(MaterielsList[i].Materiel.Type == "Network device" || MaterielsList[i].Materiel.Type == "Printer" || MaterielsList[i].Materiel.Type == "Laptop" || MaterielsList[i].Materiel.Type == "Mini Pc" || MaterielsList[i].Materiel.Type == "Monitor")
                {
                MaterielsList[i].HaveSr = true;
                AddSerielNumber(i, true);

                changeSR = true;

            }
                else
            {
                MaterielsList[i].HaveSr = false;
                changeSR = false;

            }

        }
        protected async Task UpdateMaterialType(string materialName, int index)
        {
            if (!string.IsNullOrEmpty(materialName) && MaterialSuggestions.Contains(materialName))
            {

                var material = await MaterielService.GetMaterielByName(materialName);
                if (material != null)
                {
                    MaterielsList[index].Materiel.Type = material.Type;
                    TypeSRChecker(index);
                    StateHasChanged();


                }
            }
            else
            {
                MaterielsList[index].Materiel.Type = "";
                StateHasChanged();
            }

        }
        protected void AddSerielNumber(int i, bool rerender = false)
        {
            if (!rerender)
            {
                MaterielsList[i].HaveSr = !MaterielsList[i].HaveSr;
            }

            if (MaterielsList[i].HaveSr)
            {
                int newSize = MaterielsList[i].Materiel.QuantityPDRStock;
                if (newSize > 0)
                {
                    MaterielsList[i].SRList = new List<SerialNumber>();
                    for (int j = 0; j < newSize; j++)
                    {
                        MaterielsList[i].SRList.Add(new SerialNumber { SR = "" });
                    }
                }

                else
                {
                    MaterielsList[i].SRList.Clear();
                }
            }
            else
            {
                MaterielsList[i].SRList.Clear();
            }
        }

        public async Task AddDelayedMats()
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
            var res = await DialogService.OpenAsync<ConfirmDeliveryOrder>("Confirm", new Dictionary<string, object> { { "IsDeliveryOrder", false } }, options);

            if (res == null)
            {
                return;
            }

            SelectedDeliveryOrders.HasDelayedM = res;

            foreach (var mat in MaterielsList)
            {
                if (!mat.HaveSr && SelectedDeliveryOrders.DeliveryOrderMateriels.Any(dlom => dlom.Materiel.MaterielName == mat.Materiel.MaterielName))
                {

                    await UpdateMaterialQte(mat);
                    await UpdateDLOM(mat);

                }
                else if (!mat.HaveSr)
                {
                    if (MaterialSuggestions.Contains(mat.Materiel.MaterielName))
                    {
                        int ID = await UpdateMaterialQte(mat);

                        await CreateDLOM(mat,ID);


                    }
                    else
                    {
                        mat.Materiel.Warranty = DateTime.Now.AddMonths((mat.Year * 12) + mat.Month);
                        int id = (await MaterielService.CreateMateriel(mat.Materiel)).Id;
                        await CreateDLOM(mat, id);
                    }
                }
                else 
                {
                    await CreateMaterials(mat);
                }
            }
            DialogService.Close(null);
        }

        protected async Task<int> UpdateMaterialQte(MaterielViewModel mat)
        {
            var matToUpdate = await MaterielService.GetMaterielByName(mat.Materiel.MaterielName);
            if (matToUpdate == null)
            {
                return 0;
            }
            matToUpdate.QuantityPDRStock = matToUpdate.QuantityPDRStock + mat.Materiel.QuantityPDRStock;

            int id = (await MaterielService.UpdateMateriel(matToUpdate.Id, matToUpdate)).Id;

            return id;
        }

        protected async Task UpdateDLOM(MaterielViewModel mat)
        {
            var dlomToUpdate = SelectedDeliveryOrders.DeliveryOrderMateriels.Where(dlom => dlom.Materiel.MaterielName == mat.Materiel.MaterielName).FirstOrDefault();
            if (dlomToUpdate == null)
            {
                return;
            }
            dlomToUpdate.Qte = dlomToUpdate.Qte + mat.Materiel.QuantityPDRStock;

            await DeliveryOrderMaterielService.UpdateDeliveryOrderMateriel(dlomToUpdate.MaterielId, dlomToUpdate.DeliveryOrderNumber, dlomToUpdate);

            dlomToUpdate.DeliveryOrder.Descriptoin = dlomToUpdate.DeliveryOrder.Descriptoin + "\n" + DateTime.Now + ": Added " + mat.Materiel.QuantityPDRStock + " " + mat.Materiel.MaterielName + ".";

            await DeliveryOrderService.UpdateDeliveryOrder(dlomToUpdate.DeliveryOrderNumber, dlomToUpdate.DeliveryOrder);
        }

        protected async Task CreateDLOM (MaterielViewModel mat , int id)
        {
            var dlomToUpdate = new ITStockM.Domain.Entities.DeliveryOrderMateriel
            {
                MaterielId = id,
                DeliveryOrderNumber = SelectedDeliveryOrders.DeliveryOrderNumber,
                Qte = mat.HaveSr ? 1 : mat.Materiel.QuantityPDRStock,
                DeliveryOrder = SelectedDeliveryOrders
            };
            await DeliveryOrderMaterielService.CreateDeliveryOrderMateriel(dlomToUpdate);

            dlomToUpdate.DeliveryOrder.Descriptoin = dlomToUpdate.DeliveryOrder.Descriptoin + "\n" + DateTime.Now + ": Added " + mat.Materiel.QuantityPDRStock + " " + mat.Materiel.MaterielName + ".";

            await DeliveryOrderService.UpdateDeliveryOrder(dlomToUpdate.DeliveryOrderNumber, dlomToUpdate.DeliveryOrder);
        }

        protected async Task CreateMaterials (MaterielViewModel mat)
        {
            foreach(var sr in mat.SRList)
            {
                var newMateriel = new ITStockM.Domain.Entities.Materiel
                {
                    MaterielName = mat.Materiel.MaterielName,
                    Type = mat.Materiel.Type,
                    QuantityPDRStock = 1, 


                    Warranty = DateTime.Now.AddMonths((mat.Year * 12) + mat.Month),
                    SerialNumber = sr.SR,


                    Repairing_Quantity = 0,
                    IrreparableQuantity = 0,
                };
                int id = (await MaterielService.CreateMateriel(newMateriel)).Id;
                await CreateDLOM(mat,  id);
            }
           
            
        }
    }
}