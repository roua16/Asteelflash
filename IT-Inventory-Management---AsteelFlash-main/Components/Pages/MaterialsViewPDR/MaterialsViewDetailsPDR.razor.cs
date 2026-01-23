using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsViewPDR
{
    public partial class MaterialsViewDetailsPDR
    {

        [Parameter]
        public List<Models.ITStockManagment.DeliveryOrderMateriel>  deliveryOrderMateriels { get; set; }

        protected RadzenDataGrid<Models.ITStockManagment.DeliveryOrderMateriel> grid0;

        


    

    }
}