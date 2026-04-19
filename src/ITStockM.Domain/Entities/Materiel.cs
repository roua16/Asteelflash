
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ITStockM.Domain.Base;

namespace ITStockM.Domain.Entities
{
    [Table("Materiel", Schema = "dbo")]
    public partial class Materiel : BaseEntity
    {
        [Column("materielName")]
        [Required]
        public string MaterielName { get; set; }

        [Column("type")]
        [Required]
        public string Type { get; set; }

        public string? SerialNumber { get; set; }



        [Required]
        public int QuantityITStock { get; set; }

        [Required]
        public int QuantityPDRStock { get; set; }

        [Required]
        public int IrreparableQuantity { get; set; }
        [Required]
        public int Repairing_Quantity { get; set; }

        [Column(TypeName = "datetime2")]
        [Required]
        public DateTime Warranty { get; set; }

        // ─── Lifecycle-module fields ─────────────────────────────────────────────

        /// <summary>Date the asset was purchased / first entered the system.</summary>
        [Column(TypeName = "datetime2")]
        public DateTime? PurchaseDate { get; set; }

        /// <summary>Expected useful lifetime in months (default 48 = 4 years).</summary>
        public int? ExpectedLifetimeMonths { get; set; } = 48;

        /// <summary>Latest calculated health score 0–100. Updated daily by AssetHealthBackgroundService.</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? CurrentHealthScore { get; set; }

        /// <summary>Current lifecycle stage (Purchased / InStock / Assigned / UnderMaintenance / Retired).</summary>
        [MaxLength(50)]
        public string? LifecycleStatus { get; set; } = Enums.LifecycleStage.InStock;

        // ─── Lifecycle collections ───────────────────────────────────────────────
        public ICollection<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();
        public ICollection<AssetLifecycleRecord> AssetLifecycleRecords { get; set; } = new List<AssetLifecycleRecord>();
        public ICollection<AssetPrediction> AssetPredictions { get; set; } = new List<AssetPrediction>();





        public ICollection<AssignmentMateriel> AssignmentMateriels { get; set; } = new List<AssignmentMateriel>();

        public ICollection<DeliveryOrderMateriel> DeliveryOrderMateriels { get; set; } = new List<DeliveryOrderMateriel>();
    }
}