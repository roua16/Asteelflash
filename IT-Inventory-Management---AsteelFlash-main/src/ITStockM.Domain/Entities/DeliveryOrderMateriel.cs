
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ITStockM.Domain.Base;

namespace ITStockM.Domain.Entities
{
    /// <summary>
    /// Junction table representing the many-to-many relationship between DeliveryOrders and Materiels.
    /// Inherits from BaseEntity to support standard CRUD operations via BaseCrudService.
    /// </summary>
    [Table("DeliveryOrderMateriel", Schema = "dbo")]
    public partial class DeliveryOrderMateriel : BaseEntity
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