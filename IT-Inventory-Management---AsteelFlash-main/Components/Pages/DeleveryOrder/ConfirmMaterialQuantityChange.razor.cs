using ITStockM.Components.Pages.CRUDpages;
using ITStockM.Models.ITStockManagment;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace ITStockM.Components.Pages.DeleveryOrder
{
    public partial class ConfirmMaterialQuantityChange
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Parameter]
        public DeliveryOrderMateriel DeliveryOrderMaterial { get; set; }

        [Parameter]
        public bool ToDelete { get; set; }

        protected bool isChecked;





        
        


    }
}
