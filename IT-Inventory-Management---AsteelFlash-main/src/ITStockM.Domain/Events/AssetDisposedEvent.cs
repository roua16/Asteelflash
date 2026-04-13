using ITStockM.Domain.Base;

namespace ITStockM.Domain.Events
{
    /// <summary>
    /// Raised when an asset is disposed or retired from service.
    /// Used for audit trails, compliance, and inventory cleanup.
    /// </summary>
    public class AssetDisposedEvent : DomainEvent
    {
        public int MaterielId { get; }
        public DateTime DisposalDate { get; }
        public string? Reason { get; }
        public string? DisposalMethod { get; }

        public AssetDisposedEvent(
            int materielId,
            string? reason = null,
            string? disposalMethod = null)
        {
            MaterielId = materielId;
            DisposalDate = DateTime.UtcNow;
            Reason = reason;
            DisposalMethod = disposalMethod;
        }
    }
}
