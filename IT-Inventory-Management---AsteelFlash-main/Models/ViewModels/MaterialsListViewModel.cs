
namespace ITStockM.Models.ViewModels
{
    public class MaterialsListViewModel
    {
        public string MatName { get; set; }

        public List<ITStockManagment.Materiel> Mats { get; set; }

        public int Qte { get; set; }

        public string Type { get; set; }

        public List<ITStockManagment.DeliveryOrder> DeliveryOrders { get; set; }


    }

}