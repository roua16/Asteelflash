
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ITStockM.Domain.Base;

namespace ITStockM.Domain.Entities
{
    [Table("Offer", Schema = "dbo")]
    public partial class Offer : BaseEntity
    {

        [Required]
        public int RequestId { get; set; }

        public Request? Request { get; set; }

        [Required]
        public string? SupplierName { get; set; }

        public Supplier? Supplier { get; set; }

        [Column(TypeName = "datetime2")]
        [Required]
        public DateTime DeliveryDate { get; set; }

        [Required]
        public decimal Price { get; set; }

        public bool? Selected { get; set; }
    }
}