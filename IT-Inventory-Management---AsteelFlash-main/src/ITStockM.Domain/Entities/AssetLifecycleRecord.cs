using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Domain.Entities
{
    /// <summary>
    /// Captures each stage transition in an asset's lifecycle.
    /// One row per transition; open (EndDate == null) means the asset is currently in that stage.
    /// </summary>
    [Table("AssetLifecycleRecord", Schema = "dbo")]
    public class AssetLifecycleRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int MaterielId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Stage { get; set; } = Enums.LifecycleStage.InStock;

        [Column(TypeName = "datetime2")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime? EndDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ─── Navigation ──────────────────────────────────────────────────────────
        [ForeignKey(nameof(MaterielId))]
        public Materiel Materiel { get; set; } = null!;
    }
}
