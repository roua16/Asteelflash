using Microsoft.AspNetCore.Components;
using Radzen;

namespace ITStockM.Components.Layout
{
    public partial class MainLayout
    {
        [Inject]
        protected NavigationManager NavigationManager { get; set; }
    }
}
