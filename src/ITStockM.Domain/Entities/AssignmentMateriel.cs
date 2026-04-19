
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ITStockM.Domain.Base;

namespace ITStockM.Domain.Entities
{
    /// <summary>
    /// Junction table representing the many-to-many relationship between Assignments and Materiels.
    /// Inherits from BaseEntity to support standard CRUD operations via BaseCrudService.
    /// </summary>
    [Table("AssignmentMateriel", Schema = "dbo")]
    public partial class AssignmentMateriel : BaseEntity
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