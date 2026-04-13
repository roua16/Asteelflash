
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Domain.Entities
{
    /// <summary>
    /// Junction table representing the many-to-many relationship between Assignments and Materiels.
    /// Does not inherit from BaseEntity as it is a bridge entity without identity semantics.
    /// </summary>
    [Table("AssignmentMateriel", Schema = "dbo")]
    public partial class AssignmentMateriel
    {
        [Required]
        public int MaterielId { get; set; }

        public Materiel Materiel { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        public Assignment Assignment { get; set; }

        [Required]
        public int Qte { get; set; }
    }
}