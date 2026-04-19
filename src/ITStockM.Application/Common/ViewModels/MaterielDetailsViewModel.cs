using ITStockM.Domain.Entities;

namespace ITStockM.Models.ViewModels
{
    public class MaterielDetailsViewModel
    {
        public Materiel Materiel { get; set; }

        public string DeliveryOrderNumber { get; set; }

        public string SupplierName { get; set; }
    }
}
