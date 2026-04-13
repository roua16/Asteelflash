
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Domain.Entities
{
    [Table("AssignmentMateriel", Schema = "dbo")]
    public partial class AssignmentMateriel
    {
        [Key]
        [Required]
        public int MaterielId { get; set; }

        public Materiel Materiel { get; set; }

        [Key]
        [Required]
        public int AssignmentId { get; set; }

        public Assignment Assignment { get; set; }

        [Required]
        public int Qte { get; set; }
    }
}