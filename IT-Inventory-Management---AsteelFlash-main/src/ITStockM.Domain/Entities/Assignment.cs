
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Domain.Entities
{
    [Table("Assignment", Schema = "dbo")]
    public partial class Assignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? AssignedTo { get; set; }
        
        public Employee AssignedEmployee { get; set; }

        [Required]
        public int AssignedBy { get; set; }

        public Employee Employee { get; set; }

        public int? ProjectId { get; set; }

        public Project Project { get; set; }

        [Column(TypeName="datetime2")]
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Descipriton { get; set; }

        public bool OnMission { get; set; }

        [Column(TypeName="datetime2")]
        public DateTime? RestoreDateLimit { get; set; }

        [Column(TypeName="datetime2")]
        public DateTime? RestoreDate { get; set; }

        public ICollection<AssignmentMateriel> AssignmentMateriels { get; set; }
    }
}