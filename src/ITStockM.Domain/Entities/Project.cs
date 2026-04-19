
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ITStockM.Domain.Base;

namespace ITStockM.Domain.Entities
{
    [Table("Project", Schema = "dbo")]
    public partial class Project : BaseEntity
    {

        [Required]
        public string ProjectName { get; set; }

        public ICollection<Assignment> Assignments { get; set; }
    }
}