using ITStockM.Components.Pages.CRUDpages;
using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsViewPDR
{
    public partial class MaterialsViewInterfacePDR
    {

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; }

        protected List<Models.ViewModels.MaterialsListViewModel> materielsList;

       
        protected int pendingDeliveries;
        protected int stockLeft;


        protected string search = "";


        protected RadzenDataGrid<Models.ViewModels.MaterialsListViewModel> grid0;
        protected string userAuth = "";
        protected override async Task OnInitializedAsync()
        {
            
            userAuth = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value.Role;
            var mats = (await ITStockManagmentService.GetMateriels()).Where(m => m.QuantityPDRStock != 0);
            var deliveryOrders = await ITStockManagmentService.GetDeliveryOrderMateriels(new Query
            {
                
                Expand = "DeliveryOrder, Materiel"
            });

            materielsList = mats.AsEnumerable()
                .GroupBy(dlo => dlo.MaterielName) 
                               .Select( g => new Models.ViewModels.MaterialsListViewModel
                               {
                                   MatName = g.Key,
                                   Qte = g.Sum(dlo => dlo.QuantityPDRStock),
                                   Mats = g.ToList(),
                                   Type = g.First().Type,
                                   DeliveryOrders = deliveryOrders.Where(dlo => dlo.Materiel.MaterielName == g.FirstOrDefault().MaterielName).Select(dlo => dlo.DeliveryOrder).ToList() ////lenenana
                                   
                               }).Where( m => m.Qte !=0).ToList();

            

            stockLeft = materielsList.Sum(m => m.Qte);


            
            pendingDeliveries = (await ITStockManagmentService.GetOffers()).Where ( o =>  o.Selected == true && o.DeliveryDate.CompareTo(DateTime.Today)>0 ).Count() ;


            


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

            await DialogService.OpenAsync<MaterialsViewDetailsPDR>("", new Dictionary<string, object> { { "deliveryOrderMateriels", dlom.ToList() } }, options);
        }

        protected async Task AssignToIT()
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
            var mats  = await DialogService.OpenAsync<AddMaterialsAssignmentsPDR>("", new Dictionary<string, object> {}, options);

            await OnInitializedAsync();
            search = "";




        }


        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            var mats = await ITStockManagmentService.GetMateriels(new Query { Filter = $@"i => i.MaterielName.Contains(@0) || i.Type.Contains(@0) || i.SerialNumber.Contains(@0)  ", FilterParameters = new object[] { search } });
            var deliveryOrderMateriels = await ITStockManagmentService.GetDeliveryOrderMateriels(new Query
            {
                Expand = "DeliveryOrder, Materiel"
            });
            materielsList = mats
                .AsEnumerable()
                .GroupBy(dlo => dlo.MaterielName) 
                               .Select(g => new ITStockM.Models.ViewModels.MaterialsListViewModel
                               {
                                   MatName = g.Key,
                                   Qte = g.Sum(dlo => dlo.QuantityPDRStock),
                                   Mats = g.ToList(),
                                   Type = g.First().Type,
                                   DeliveryOrders = deliveryOrderMateriels.Where(dlo => dlo.Materiel.MaterielName == g.FirstOrDefault().MaterielName).Select(dlo => dlo.DeliveryOrder).ToList() ////lenenana

                               }).Where(m => m.Qte != 0).ToList();

        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {

            if (args?.Value == "csv")
            {
                await ITStockManagmentService.ExportMaterielsPDRToCSV(new Query { }, "PDR - Materials");
            }
            else if (args == null || args.Value == "xlsx")
            {
                await ITStockManagmentService.ExportMaterielsPDRToExcel(new Query { }, "PDR - Materials");
            }
        }

        protected void NavToPendingDeliveries()
        {
            if (userAuth != "Admin")
            {
                NavigationManager.NavigateTo("/pending-deliveries");

            }
            
        }

    }

}