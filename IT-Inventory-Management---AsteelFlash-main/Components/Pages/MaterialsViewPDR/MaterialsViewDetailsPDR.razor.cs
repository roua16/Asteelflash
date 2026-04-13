using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsViewPDR
{
    public partial class MaterialsViewDetailsPDR
    {

        [Parameter]
        public List<Domain.Entities.DeliveryOrderMateriel>  deliveryOrderMateriels { get; set; }

        protected RadzenDataGrid<Domain.Entities.DeliveryOrderMateriel> grid0;

        


    

    }
}