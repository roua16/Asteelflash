using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsView
{
    public partial class MaterialsViewDetails
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; } = default!;

        [Parameter]
        public List<Domain.Entities.DeliveryOrderMateriel> deliveryOrderMateriels { get; set; } = new();

        [Parameter]
        public string CurrentCondition { get; set; } = string.Empty;

        protected RadzenDataGrid<Domain.Entities.DeliveryOrderMateriel> grid0 = default!;


        protected string Role = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            var userSession = await LocalStorage.GetAsync<UserSession>("UserSession");
            Role = userSession.Success && userSession.Value is not null
                ? userSession.Value.Role ?? string.Empty
                : string.Empty;
        }

        protected async Task ChangeCondition()
        {
            var firstMateriel = deliveryOrderMateriels.FirstOrDefault()?.Materiel;
            if (firstMateriel is null)
            {
                return;
            }

            var options = new DialogOptions
            {
                Style = "min-width: 600px;",
                CssClass = "dialog-animation",
                CloseDialogOnOverlayClick = true,
                Resizable = true,
                Draggable = true,
                CloseDialogOnEsc = true
            };

            var updated = await DialogService.OpenAsync<ConfirmConditionChange>(
                "",
                new Dictionary<string, object>
                {
                    { "MaxQte", firstMateriel.QuantityITStock },
                    { "HasSN", !string.IsNullOrEmpty(firstMateriel.SerialNumber) },
                    { "materiels", deliveryOrderMateriels.Select(dlm => dlm.Materiel).Where(m => m.QuantityITStock != 0).ToList() },
                    { "CurrentCondition", CurrentCondition }
                },
                options);

            if (updated is bool isUpdated && isUpdated)
            {
                DialogService.Close(true);
            }
        }

    }
}