
using ITStockM.Domain.Entities;

namespace ITStockM.Models.ViewModels
{
    public class MaterialsListViewModel
    {
        public string MatName { get; set; }

        public List<Materiel> Mats { get; set; }

        public int Qte { get; set; }

        public string Type { get; set; }

        public List<DeliveryOrder> DeliveryOrders { get; set; }


    }

}