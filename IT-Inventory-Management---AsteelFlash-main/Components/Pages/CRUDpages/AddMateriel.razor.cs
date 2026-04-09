using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;
using ITStockM.Services.Materiels;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class AddMateriel
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IMaterielService MaterielService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            materiel = new Models.ITStockManagment.Materiel();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.Materiel materiel = new();

        protected async Task FormSubmit()
        {
            try
            {
                await MaterielService.CreateMateriel(materiel);
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