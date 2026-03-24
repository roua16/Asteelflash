using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Models.ITStockManagment
{
    [Table("MaintenanceTicket", Schema = "dbo")]
    public class MaintenanceTicket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int MaterielId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string ProblemDescription { get; set; } = string.Empty;

        public int? ReportedByEmployeeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = Enums.MaintenanceTicketStatus.Open;

        /// <summary>Repair / labour cost in EUR.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cost { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime? ResolvedAt { get; set; }

        [MaxLength(2000)]
        public string? Resolution { get; set; }

        // ─── Navigation ──────────────────────────────────────────────────────────
        [ForeignKey(nameof(MaterielId))]
        public Materiel Materiel { get; set; } = null!;

        [ForeignKey(nameof(ReportedByEmployeeId))]
        public Employee? ReportedBy { get; set; }
    }
}
