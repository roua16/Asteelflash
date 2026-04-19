
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ITStockM.Domain.Base;

namespace ITStockM.Domain.Entities
{
    [Table("Supplier", Schema = "dbo")]
    public partial class Supplier : BaseEntity
    {
        [Required]
        public string? SupplierName { get; set; }

        [Required]
        public string? Adress { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? PhoneNumber { get; set; }

        public ICollection<DeliveryOrder> DeliveryOrders { get; set; } = new List<DeliveryOrder>();

        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    }
}