using ITStockM.Models.ViewModels;
using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.DeleveryOrder
{
    public partial class DeliveryOrder
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        private ElementReference formElement;
        private RadzenTemplateForm<Models.ITStockManagment.DeliveryOrder> form;

        protected bool errorVisible = false;
        protected Models.ITStockManagment.DeliveryOrder deliveryOrder = new Models.ITStockManagment.DeliveryOrder();
        protected List<MaterielViewModel> MaterielsList = new List<MaterielViewModel>();
        protected List<Models.ITStockManagment.Supplier> suppliersForSupplierName;
        protected List<string> options = new List<string> { "HardWare", "SoftWare", "Mouse", "KeyBoard", "Laptop", "Mini Pc", "Backpack", "Headphone", "Monitor", "Network device", "Printer", "Consumables" };
        protected bool changeSR = true;

        private IEnumerable<string> MaterialSuggestions { get; set; }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("preventEnterKeyFormSubmission");
            }
        }

        protected override async Task OnInitializedAsync()
        {
            deliveryOrder = new Models.ITStockManagment.DeliveryOrder();
            suppliersForSupplierName = await ITStockManagmentService.GetSuppliersList();


            MaterielsList = new List<MaterielViewModel>
                            {
                                new MaterielViewModel
                                {
                                    Materiel = new Models.ITStockManagment.Materiel { MaterielName = "",  QuantityPDRStock = 1, QuantityITStock = 0 , IrreparableQuantity =0, Repairing_Quantity =0},
                                    HaveSr = false,
                                    SRList = new List<SerialNumber>(),
                                    Year = 0,
                                    Month = 0
                                }
                            };

            var mats = await ITStockManagmentService.GetMateriels();

            MaterialSuggestions = mats.Select(m => m.MaterielName).Distinct();



        }

        protected void TypeSRChecker(int i)
        {

            if (MaterielsList[i].Materiel.Type == "Network device" || MaterielsList[i].Materiel.Type == "Printer" || MaterielsList[i].Materiel.Type == "Laptop" || MaterielsList[i].Materiel.Type == "Mini Pc" || MaterielsList[i].Materiel.Type == "Monitor")
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

        protected void AddMateriel()
        {

            var newMateriel = new MaterielViewModel
            {
                Materiel = new Models.ITStockManagment.Materiel { MaterielName = "", QuantityPDRStock = 1, QuantityITStock = 0, IrreparableQuantity = 0, Repairing_Quantity = 0 },
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

        protected async Task AddDeliveryOrderAsync()
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
            var res = await DialogService.OpenAsync<ConfirmDeliveryOrder>("Confirm", new Dictionary<string, object> { { "IsDeliveryOrder", true } }, options);

            if (res == null)
            {
                return;
            }



            await InvokeAsync(async () =>
            {
                deliveryOrder.Date = DateTime.Now;
                deliveryOrder.HasDelayedM = res;
                deliveryOrder.EmployeeId = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value.Id;
                try
                {


                    await ITStockManagmentService.CreateDeliveryOrder(deliveryOrder);



                    foreach (var materiel in MaterielsList)
                    {



                        Models.ITStockManagment.DeliveryOrderMateriel deliveryOrderMateriel = new Models.ITStockManagment.DeliveryOrderMateriel
                        {
                            DeliveryOrderNumber = deliveryOrder.DeleveryOrderNumber,

                            Qte = materiel.Materiel.QuantityPDRStock
                        };




                        materiel.Materiel.Warranty = DateTime.Now.AddMonths((materiel.Year * 12) + materiel.Month);

                        if (materiel.HaveSr)
                        {

                            foreach (var sr in materiel.SRList)
                            {



                                var newMateriel = new Models.ITStockManagment.Materiel
                                {
                                    MaterielName = materiel.Materiel.MaterielName,
                                    Type = materiel.Materiel.Type,
                                    QuantityPDRStock = 1,


                                    Warranty = DateTime.Now.AddMonths((materiel.Year * 12) + materiel.Month),
                                    SerialNumber = sr.SR,


                                    Repairing_Quantity = 0,
                                    IrreparableQuantity = 0,
                                };

                                if (newMateriel.MaterielName == "" || newMateriel.Type == "" || newMateriel.SerialNumber == "")
                                {
                                    throw new Exception("Materiel name, Type and SerialNumber is required");
                                }
                                deliveryOrderMateriel.MaterielId = (await ITStockManagmentService.CreateMateriel(newMateriel)).Id;

                                deliveryOrderMateriel.Qte = 1;
                                await ITStockManagmentService.CreateDeliveryOrderMateriel(deliveryOrderMateriel);

                            }
                        }
                        else
                        {

                            if (materiel.Materiel.MaterielName == "" || materiel.Materiel.Type == "")
                            {
                                throw new Exception("Materiel name is required");
                            }


                            if (MaterialSuggestions.Contains(materiel.Materiel.MaterielName))
                            {
                                Models.ITStockManagment.Materiel oldMat = await ITStockManagmentService.GetMaterielByName(materiel.Materiel.MaterielName);

                                oldMat.QuantityPDRStock = oldMat.QuantityPDRStock + materiel.Materiel.QuantityPDRStock;
                                deliveryOrderMateriel.MaterielId = oldMat.Id;

                                await ITStockManagmentService.UpdateMateriel(oldMat.Id, oldMat);

                                await ITStockManagmentService.CreateDeliveryOrderMateriel(deliveryOrderMateriel);

                            }
                            else
                            {
                                deliveryOrderMateriel.MaterielId = (await ITStockManagmentService.CreateMateriel(materiel.Materiel)).Id;


                                await ITStockManagmentService.CreateDeliveryOrderMateriel(deliveryOrderMateriel);
                            }



                        }


                    }




                    errorVisible = false;
                }

                catch (Exception ex)
                {
                    errorVisible = true;
                    StateHasChanged();

                    await JSRuntime.InvokeVoidAsync("scrollToElement", "scrollback");

                }

                DialogService.Close();


                if (!errorVisible)
                {
                    MaterielsList = new List<MaterielViewModel>
                    {
                        new MaterielViewModel
                        {
                            Materiel = new Models.ITStockManagment.Materiel { MaterielName = "", Type = "", QuantityPDRStock = 1, QuantityITStock=0,   Repairing_Quantity = 0, IrreparableQuantity=0},
                            HaveSr = false,
                            SRList = new List<SerialNumber>()
                        }
                    };
                    deliveryOrder = new Models.ITStockManagment.DeliveryOrder();

                    await JSRuntime.InvokeVoidAsync("scrollToElement", "scrollback");
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Success",
                        Detail = "Delivery order saved successfully",
                        Duration = 4000
                    });

                    StateHasChanged();

                }



            });

            await DialogService.OpenAsync<LoadingScreen>("", null
               , new DialogOptions() { ShowTitle = false, Style = "width:100%;height:100%;", CloseDialogOnEsc = false });

        }


        protected async Task AddSupplier()
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

            var result = await DialogService.OpenAsync<Supplier.AddSupplier>("", null, options);
            if (result != null)
            {
                suppliersForSupplierName.Add(result);
            }

        }


        protected async Task UpdateMaterialType(string materialName, int index)
        {
            if (!string.IsNullOrEmpty(materialName) && MaterialSuggestions.Contains(materialName))
            {

                var material = await ITStockManagmentService.GetMaterielByName(materialName);
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









    }
}