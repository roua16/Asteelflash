using ITStockM.Domain.Base;

namespace ITStockM.Domain.Events
{
    /// <summary>
    /// Raised when a new maintenance ticket is created for an asset.
    /// Used for notifications and tracking maintenance history.
    /// </summary>
    public class MaintenanceTicketCreatedEvent : DomainEvent
    {
        public int TicketId { get; }
        public int MaterielId { get; }
        public string Description { get; }
        public DateTime CreatedDate { get; }

        public MaintenanceTicketCreatedEvent(
            int ticketId,
            int materielId,
            string description)
        {
            TicketId = ticketId;
            MaterielId = materielId;
            Description = description;
            CreatedDate = DateTime.UtcNow;
        }
    }
}
