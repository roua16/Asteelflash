using ITStockM.Domain.Base;

namespace ITStockM.Domain.Events
{
    /// <summary>
    /// Raised when warranty is expiring for an asset.
    /// Used for proactive maintenance scheduling and notifications.
    /// </summary>
    public class WarrantyExpiringEvent : DomainEvent
    {
        public int MaterielId { get; }
        public DateTime WarrantyEndDate { get; }
        public int DaysUntilExpiry { get; }

        public WarrantyExpiringEvent(int materielId, DateTime warrantyEndDate)
        {
            MaterielId = materielId;
            WarrantyEndDate = warrantyEndDate;
            DaysUntilExpiry = (int)(warrantyEndDate - DateTime.UtcNow).TotalDays;
        }
    }
}
