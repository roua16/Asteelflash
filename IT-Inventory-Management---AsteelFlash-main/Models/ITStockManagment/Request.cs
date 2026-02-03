
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Models.ITStockManagment
{
    [Table("Request", Schema = "dbo")]
    public partial class Request
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? ProjectName { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        public string? MaterialType { get; set; }

        [Column("date", TypeName = "datetime2")]
        [Required]
        public DateTime Date { get; set; }

        [Column("status")]
        [Required]
        public string? Status { get; set; }

        public byte[]? File { get; set; }

        public string? FileExtension { get; set; }

        public string? FileName { get; set; }

        [Column("approvedAt", TypeName = "datetime2")]
        public DateTime? ApprovedAt { get; set; }

        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    }
}