using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Domain.Entities
{
    /// <summary>
    /// Stores the most recent health prediction for an asset,
    /// updated daily by <c>AssetHealthBackgroundService</c>.
    /// </summary>
    [Table("AssetPrediction", Schema = "dbo")]
    public class AssetPrediction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int MaterielId { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>0–100. Higher is healthier.</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal HealthScore { get; set; }

        [Required]
        [MaxLength(20)]
        public string HealthStatus { get; set; } = Enums.HealthStatus.Good;

        /// <summary>Estimated date the asset may fail (null if asset is healthy).</summary>
        [Column(TypeName = "datetime2")]
        public DateTime? PredictedFailureDate { get; set; }

        /// <summary>Recommended date to procure a replacement.</summary>
        [Column(TypeName = "datetime2")]
        public DateTime? RecommendedReplacementDate { get; set; }

        [MaxLength(1000)]
        public string? RecommendationReason { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedReplacementCost { get; set; }

        // ─── Navigation ──────────────────────────────────────────────────────────
        [ForeignKey(nameof(MaterielId))]
        public Materiel Materiel { get; set; } = null!;
    }
}
