using Microsoft.AspNetCore.Components;
using Radzen;

namespace ITStockM.Components.Pages.MaterialsAssignments
{
    public partial class ConfirmMaterialReturn
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Parameter]
        public bool HasSN { get; set; }

        [Parameter]
        public int MaxQte { get; set; }

        [Parameter]
        public string MaterialName { get; set; } = string.Empty;

  
        protected List<string> options = new List<string> { "Good Conditions", "Need to be repaired", "Unrepairable" };

        protected string selectedOption = "Good Conditions";

        protected int qte = 1;

        protected string description = string.Empty;
        protected string otherName = string.Empty;
        protected int otherQty;

        public void Submit()
        {
            
            description = "\n--\n" + qte + " " +MaterialName + ", Returned at: " + DateTime.Now + " ("+selectedOption+")"+"\n";
            var result = new Dictionary<string, object>
            {
                { "selectedOption", selectedOption },
                { "qte",  qte },
                { "description", description },
                { "otherName", otherName },
                { "otherQty", otherQty }
            };
           
            DialogService.Close(result);
        }
    }
}
