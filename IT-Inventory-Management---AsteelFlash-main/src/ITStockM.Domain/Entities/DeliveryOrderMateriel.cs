
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Domain.Entities
{
    [Table("DeliveryOrderMateriel", Schema = "dbo")]
    public partial class DeliveryOrderMateriel
    {
        [Key]
        [Required]
        public int MaterielId { get; set; }

        public Materiel? Materiel { get; set; }

        [Key]
        [Required]
        public string? DeliveryOrderNumber { get; set; }

        public DeliveryOrder? DeliveryOrder { get; set; }

        [Required]
        public int Qte { get; set; }
    }
}