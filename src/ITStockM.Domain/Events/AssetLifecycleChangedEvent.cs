using ITStockM.Domain.Base;

namespace ITStockM.Domain.Events
{
    /// <summary>
    /// Raised when an asset transitions between lifecycle stages.
    /// Used for audit trails and notifications about asset status changes.
    /// </summary>
    public class AssetLifecycleChangedEvent : DomainEvent
    {
        public int MaterielId { get; }
        public string PreviousStage { get; }
        public string NewStage { get; }
        public DateTime TransitionDate { get; }
        public string? Reason { get; }

        public AssetLifecycleChangedEvent(
            int materielId,
            string previousStage,
            string newStage,
            string? reason = null)
        {
            MaterielId = materielId;
            PreviousStage = previousStage;
            NewStage = newStage;
            TransitionDate = DateTime.UtcNow;
            Reason = reason;
        }
    }
}
