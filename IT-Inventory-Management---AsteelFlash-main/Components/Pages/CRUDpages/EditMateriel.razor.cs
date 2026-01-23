
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class EditMateriel
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public int Id { get; set; }

        protected override async Task OnInitializedAsync()
        {
            materiel = await ITStockManagmentService.GetMaterielById(Id);
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.Materiel materiel;

        protected async Task FormSubmit()
        {
            try
            {
                await ITStockManagmentService.UpdateMateriel(Id, materiel);
                DialogService.Close(materiel);
            }
            catch (Exception ex)
            {
                errorVisible = true;
            }
        }

        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close(null);
        }
    }
}