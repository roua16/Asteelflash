
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Domain.Entities
{
    /// <summary>
    /// Junction table representing the many-to-many relationship between DeliveryOrders and Materiels.
    /// Does not inherit from BaseEntity as it is a bridge entity without identity semantics.
    /// </summary>
    [Table("DeliveryOrderMateriel", Schema = "dbo")]
    public partial class DeliveryOrderMateriel
    {
        [Required]
        public int MaterielId { get; set; }

        public Materiel? Materiel { get; set; }

        [Required]
        public string? DeliveryOrderNumber { get; set; }

        public DeliveryOrder? DeliveryOrder { get; set; }

        [Required]
        public int Qte { get; set; }
    }
}