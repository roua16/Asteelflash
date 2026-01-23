
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Models.ITStockManagment
{
    [Table("Project", Schema = "dbo")]
    public partial class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string ProjectName { get; set; }

        public ICollection<Assignment> Assignments { get; set; }
    }
}