
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Models.ITStockManagment
{
    [Table("Materiel", Schema = "dbo")]
    public partial class Materiel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("materielName")]
        [Required]
        public string MaterielName { get; set; }

        [Column("type")]
        [Required]
        public string Type { get; set; }

        public string SerialNumber { get; set; }

        

        [Required]
        public int QuantityITStock { get; set; }

        [Required]
        public int QuantityPDRStock { get; set; }

        [Required]
        public int IrreparableQuantity { get; set; }
        [Required]
        public int Repairing_Quantity { get; set; }

        [Column(TypeName="datetime2")]
        [Required]
        public DateTime Warranty { get; set; }

        





        public ICollection<AssignmentMateriel> AssignmentMateriels { get; set; }

        public ICollection<DeliveryOrderMateriel> DeliveryOrderMateriels { get; set; }
    }
}