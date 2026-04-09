
using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsView
{
    public partial class MaterialsViewInterface
    {

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; } = default!;

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; } = default!;

        protected IEnumerable<Models.ViewModels.MaterialsListViewModel> materielsList = Enumerable.Empty<Models.ViewModels.MaterialsListViewModel>();

        protected int pendingRequests;
        protected int pendingDeliveries;
        protected int stockLeft;
        protected int runingOutOfStock;

        protected List<string> materialStorage = new List<string> { "All", "IT", "PDR", "Repairing", "Irreparable" };

        protected string selectedStorage = "IT";

        protected string search = "";

        protected string userAuth = string.Empty;

        protected RadzenDataGrid<Models.ViewModels.MaterialsListViewModel> grid0 = default!;

        protected override async Task OnInitializedAsync()
        {

            userAuth = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value?.Role ?? string.Empty;

            var mats = (await ITStockManagmentService.GetMateriels()).Where(m => m.QuantityITStock != 0 || m.QuantityPDRStock !=0);
            var deliveryOrders = await ITStockManagmentService.GetDeliveryOrderMateriels(new Query
            {

                Expand = "DeliveryOrder, Materiel"
            });

            materielsList = mats.AsEnumerable().GroupBy(m => m.MaterielName)
                               .Select(g => new ITStockM.Models.ViewModels.MaterialsListViewModel
                               {
                                   MatName = g.Key,
                                   Qte = g.Sum(m =>  m.QuantityITStock),
                                   Mats = g.ToList(),
                                   Type = g.First().Type,
                                   DeliveryOrders = GetDeliveryOrdersForGroup(deliveryOrders, g)

                               }).Where(ml => ml.Qte != 0);



            stockLeft = mats.Sum(m => m.QuantityPDRStock + m.QuantityITStock);


            pendingRequests = (await ITStockManagmentService.GetRequests()).Where(r => r.Status != "Done").Count();


            pendingDeliveries = (await ITStockManagmentService.GetOffers()).Where(o => o.Selected == true && o.DeliveryDate.CompareTo(DateTime.Today) > 0).Count();

            runingOutOfStock = mats.Where(m => m.QuantityITStock != 0 || m.QuantityPDRStock != 0).GroupBy(m => m.MaterielName).Select(g => new { nb = g.Sum(m => m.QuantityPDRStock + m.QuantityITStock) }).Where(m => m.nb < 10).Count();




        }

        protected async Task MaterialsDetails(List<int> matIds)
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

            var filter = $"i => {string.Join(" || ", matIds.Select(id => $"i.MaterielId == {id}"))}";

            var dlom = await ITStockManagmentService.GetDeliveryOrderMateriels(new Query
            {
                Filter = filter,
                Expand = "DeliveryOrder, Materiel"
            });

            var updated = await DialogService.OpenAsync<MaterialsViewDetails>("", new Dictionary<string, object> { { "deliveryOrderMateriels", dlom.ToList() } ,{ "CurrentCondition", selectedStorage } }, options);

            if (updated != null && updated == true)
            {
                await OnInitializedAsync();
            }
        }

        protected async Task Search(ChangeEventArgs args)
        {
            search = args.Value?.ToString() ?? string.Empty;

            await grid0.GoToPage(0);
            var mats = await ITStockManagmentService.GetMateriels(new Query { Filter = $@"i => i.MaterielName.Contains(@0) || i.Type.Contains(@0) || i.SerialNumber.Contains(@0)  ", FilterParameters = new object[] { search } });
            var deliveryOrderMateriels = await ITStockManagmentService.GetDeliveryOrderMateriels(new Query
            {
                Expand = "DeliveryOrder, Materiel"
            });

            if (selectedStorage == "All")
            {

                materielsList = mats.AsEnumerable().Where(m => m.QuantityPDRStock != 0 || m.QuantityITStock != 0)
                    .GroupBy(m => m.MaterielName)
                                  .Select(g => new ITStockM.Models.ViewModels.MaterialsListViewModel
                                  {
                                      MatName = g.Key,
                                      Qte = g.Sum(dlo => dlo.QuantityPDRStock + dlo.QuantityITStock),
                                      Mats = g.ToList(),
                                      Type = g.First().Type,
                                      DeliveryOrders = GetDeliveryOrdersForGroup(deliveryOrderMateriels, g)
                                  });
            }
            else if (selectedStorage == "PDR")
            {
                materielsList = mats.AsEnumerable().Where(m => m.QuantityPDRStock != 0)
               .GroupBy(dlo => dlo.MaterielName) 
                              .Select(g => new ITStockM.Models.ViewModels.MaterialsListViewModel
                              {
                                  MatName = g.Key,
                                  Qte = g.Sum(dlo => dlo.QuantityPDRStock),
                                  Mats = g.ToList(),
                                  Type = g.First().Type,
                                  DeliveryOrders = GetDeliveryOrdersForGroup(deliveryOrderMateriels, g)

                              });
            }
            else if (selectedStorage == "IT")
            {
                materielsList = mats.AsEnumerable().Where(m => m.QuantityITStock != 0 )
              .GroupBy(dlo => dlo.MaterielName) 
                             .Select(g => new ITStockM.Models.ViewModels.MaterialsListViewModel
                             {
                                 MatName = g.Key,
                                 Qte = g.Sum(gItem => gItem.QuantityITStock),
                                 Mats = g.ToList(),
                                 Type = g.First().Type,
                                 DeliveryOrders = GetDeliveryOrdersForGroup(deliveryOrderMateriels, g)

                             });
            }
            else if (selectedStorage == "Irreparable")
            {
                materielsList = mats.AsEnumerable()
               .GroupBy(m => m.MaterielName)
                              .Select(g => new ITStockM.Models.ViewModels.MaterialsListViewModel
                              {
                                  MatName = g.Key,
                                  Qte = g.Sum(m => m.IrreparableQuantity),
                                  Mats = g.ToList(),
                                  Type = g.First().Type,
                                  DeliveryOrders = GetDeliveryOrdersForGroup(deliveryOrderMateriels, g)

                              }).Where(m => m.Qte != 0);
            }
            else if (selectedStorage == "Repairing")
            {
                materielsList = mats.AsEnumerable()
               .GroupBy(m => m.MaterielName)
                              .Select(g => new ITStockM.Models.ViewModels.MaterialsListViewModel
                              {
                                  MatName = g.Key,
                                  Qte = g.Sum(m => m.Repairing_Quantity),
                                  Mats = g.ToList(),
                                  Type = g.First().Type,
                                  DeliveryOrders = GetDeliveryOrdersForGroup(deliveryOrderMateriels, g)

                              }).Where(m => m.Qte != 0);
            }




        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (selectedStorage == "All")
            {
                var query = new Query
                {
                    Filter = "a => a.QuantityITStock != 0 || a.QuantityPDRStock != 0 || a.IrreparableQuantity != 0 || a.Repairing_Quantity != 0",
                    Select = "MaterielName, Type, SerialNumber, QuantityITStock, QuantityPDRStock, Warranty, IrreparableQuantity, Repairing_Quantity"
                };

                if (args?.Value == "csv")
                {
                    await ITStockManagmentService.ExportMaterielsToCSV(query, "All Materials");
                }
                else if (args == null || args.Value == "xlsx")
                {
                    await ITStockManagmentService.ExportMaterielsToExcel(query, "All Materials");
                }
            }

            else if (selectedStorage == "PDR")
            {
                

                if (args?.Value == "csv")
                {
                    await ITStockManagmentService.ExportMaterielsPDRToCSV(new Query{ }, "PDR - Materials");
                }
                else if (args == null || args.Value == "xlsx")
                {
                    await ITStockManagmentService.ExportMaterielsPDRToExcel(new Query { }, "PDR - Materials");
                }
            }
            else
            {

                if (args?.Value == "csv")
                {
                    await ITStockManagmentService.ExportMaterielsITToCSV(new Query { }, "IT - Materials");
                }
                else if (args == null || args.Value == "xlsx")
                {
                    await ITStockManagmentService.ExportMaterielsITToExcel(new Query { }, "IT - Materials");
                }
            }
        }
       

        protected void NavToRequests()
        {

            if (userAuth != "Admin")
            {
                NavigationManager.NavigateTo("/purchase-page");
            }
        }

        protected void NavToPendingDeliveries()
        {

            if (userAuth != "Admin")
            {
                NavigationManager.NavigateTo("/pending-deliveries");
            }
        }

        protected async Task MaterialFilter()
        {
            var mats = await ITStockManagmentService.GetMateriels();
            var deliveryOrders = await ITStockManagmentService.GetDeliveryOrderMateriels(new Query
            {

                Expand = "DeliveryOrder, Materiel"
            });
            if (selectedStorage == "All")
            {
                
                materielsList = mats.AsEnumerable().GroupBy(m => m.MaterielName) 
                                   .Select(g => new Models.ViewModels.MaterialsListViewModel
                                   {
                                       MatName = g.Key,
                                       Qte = g.Sum(m => m.QuantityPDRStock + m.QuantityITStock),
                                       Mats = g.ToList(),
                                       Type = g.First().Type,
                                       DeliveryOrders = GetDeliveryOrdersForGroup(deliveryOrders, g)

                                   }).Where( m => m.Qte != 0);
            }
            else if(selectedStorage == "IT")
            {
                
                materielsList = mats.AsEnumerable()
               .GroupBy(dlo => dlo.MaterielName) 
                              .Select(g => new Models.ViewModels.MaterialsListViewModel
                              {
                                  MatName = g.Key,
                                  Qte =  g.Sum(m => m.QuantityITStock),
                                  Mats = g.ToList(),
                                  Type = g.First().Type,
                                  DeliveryOrders = GetDeliveryOrdersForGroup(deliveryOrders, g)

                              }).Where(m => m.Qte != 0);
            }
            else if (selectedStorage == "PDR")
            {
                
                materielsList = mats.AsEnumerable()
               .GroupBy(m => m.MaterielName) 
                              .Select(g => new Models.ViewModels.MaterialsListViewModel
                              {
                                  MatName = g.Key,
                                  Qte = g.Sum(m => m.QuantityPDRStock) ,
                                  Mats = g.ToList(),
                                  Type = g.First().Type,
                                  DeliveryOrders = GetDeliveryOrdersForGroup(deliveryOrders, g)

                              }).Where(m => m.Qte != 0);
            }
            else if(selectedStorage == "Irreparable")
            {
                materielsList = mats.AsEnumerable()
               .GroupBy(m => m.MaterielName)
                              .Select(g => new Models.ViewModels.MaterialsListViewModel
                              {
                                  MatName = g.Key,
                                  Qte = g.Sum(m => m.IrreparableQuantity),
                                  Mats = g.ToList(),
                                  Type = g.First().Type,
                                  DeliveryOrders = GetDeliveryOrdersForGroup(deliveryOrders, g)

                              }).Where(m => m.Qte != 0);
            }
            else if (selectedStorage == "Repairing") {
                materielsList = mats.AsEnumerable()
               .GroupBy(m => m.MaterielName)
                              .Select(g => new Models.ViewModels.MaterialsListViewModel
                              {
                                  MatName = g.Key,
                                  Qte = g.Sum(m => m.Repairing_Quantity),
                                  Mats = g.ToList(),
                                  Type = g.First().Type,
                                  DeliveryOrders = GetDeliveryOrdersForGroup(deliveryOrders, g)

                              }).Where(m => m.Qte != 0);
            }


        }

        private static List<Models.ITStockManagment.DeliveryOrder> GetDeliveryOrdersForGroup(
            IEnumerable<Models.ITStockManagment.DeliveryOrderMateriel> deliveryOrderMateriels,
            IGrouping<string, Models.ITStockManagment.Materiel> group)
        {
            var materielName = group.FirstOrDefault()?.MaterielName;
            if (string.IsNullOrWhiteSpace(materielName))
            {
                return new List<Models.ITStockManagment.DeliveryOrder>();
            }

            return deliveryOrderMateriels
                .Where(dlo => dlo.Materiel != null && dlo.DeliveryOrder != null && string.Equals(dlo.Materiel.MaterielName, materielName, StringComparison.OrdinalIgnoreCase))
                .Select(dlo => dlo.DeliveryOrder!)
                .ToList();
        }


       

    }

}