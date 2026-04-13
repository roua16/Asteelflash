
using ITStockM.Services.DeliveryOrderMateriels;
using ITStockM.Services.Export;
using ITStockM.Services.Materiels;
using ITStockM.Services.Offers;
using ITStockM.Services.Requests;
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
        public IMaterielService MaterielService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderMaterielService DeliveryOrderMaterielService { get; set; } = default!;

        [Inject]
        public IRequestService RequestService { get; set; } = default!;

        [Inject]
        public IOfferService OfferService { get; set; } = default!;

        [Inject]
        public IExportService ExportService { get; set; } = default!;

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

            var mats = (await MaterielService.GetMateriels()).Where(m => m.QuantityITStock != 0 || m.QuantityPDRStock !=0);
            var deliveryOrders = await DeliveryOrderMaterielService.GetDeliveryOrderMateriels(new Query
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


            pendingRequests = (await RequestService.GetRequests()).Where(r => r.Status != "Done").Count();


            pendingDeliveries = (await OfferService.GetOffers()).Where(o => o.Selected == true && o.DeliveryDate.CompareTo(DateTime.Today) > 0).Count();

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

            var dlom = await DeliveryOrderMaterielService.GetDeliveryOrderMateriels(new Query
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
            var mats = await MaterielService.GetMateriels(new Query { Filter = $@"i => i.MaterielName.Contains(@0) || i.Type.Contains(@0) || i.SerialNumber.Contains(@0)  ", FilterParameters = new object[] { search } });
            var deliveryOrderMateriels = await DeliveryOrderMaterielService.GetDeliveryOrderMateriels(new Query
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
                    await ExportService.ExportToCSV("export/itstockmanagment/materiels", query, "All Materials");
                }
                else if (args == null || args.Value == "xlsx")
                {
                    await ExportService.ExportToExcel("export/itstockmanagment/materiels", query, "All Materials");
                }
            }

            else if (selectedStorage == "PDR")
            {
                

                if (args?.Value == "csv")
                {
                    await ExportService.ExportToCSV("export/itstockmanagment/materielspdr", new Query { }, "PDR - Materials");
                }
                else if (args == null || args.Value == "xlsx")
                {
                    await ExportService.ExportToExcel("export/itstockmanagment/materielspdr", new Query { }, "PDR - Materials");
                }
            }
            else
            {

                if (args?.Value == "csv")
                {
                    await ExportService.ExportToCSV("export/itstockmanagment/materielsit", new Query { }, "IT - Materials");
                }
                else if (args == null || args.Value == "xlsx")
                {
                    await ExportService.ExportToExcel("export/itstockmanagment/materielsit", new Query { }, "IT - Materials");
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
            var mats = await MaterielService.GetMateriels();
            var deliveryOrders = await DeliveryOrderMaterielService.GetDeliveryOrderMateriels(new Query
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

        private static List<Domain.Entities.DeliveryOrder> GetDeliveryOrdersForGroup(
            IEnumerable<Domain.Entities.DeliveryOrderMateriel> deliveryOrderMateriels,
            IGrouping<string, Domain.Entities.Materiel> group)
        {
            var materielName = group.FirstOrDefault()?.MaterielName;
            if (string.IsNullOrWhiteSpace(materielName))
            {
                return new List<Domain.Entities.DeliveryOrder>();
            }

            return deliveryOrderMateriels
                .Where(dlo => dlo.Materiel != null && dlo.DeliveryOrder != null && string.Equals(dlo.Materiel.MaterielName, materielName, StringComparison.OrdinalIgnoreCase))
                .Select(dlo => dlo.DeliveryOrder!)
                .ToList();
        }


       

    }

}