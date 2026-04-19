using ITStockM.Domain.Entities;

namespace ITStockM.Services.Interfaces
{
    /// <summary>
    /// Handles all maintenance-ticket CRUD and workflow operations.
    /// </summary>
    public interface IMaintenanceService
    {
        Task<IEnumerable<MaintenanceTicket>> GetAllTicketsAsync(CancellationToken ct = default);

        Task<IEnumerable<MaintenanceTicket>> GetTicketsByMaterielAsync(int materielId, CancellationToken ct = default);

        Task<MaintenanceTicket?> GetTicketByIdAsync(int id, CancellationToken ct = default);

        Task<MaintenanceTicket> CreateTicketAsync(MaintenanceTicket ticket, CancellationToken ct = default);

        Task<MaintenanceTicket> UpdateTicketAsync(int id, MaintenanceTicket ticket, CancellationToken ct = default);

        /// <summary>Marks a ticket as Closed, records the resolution text and optional cost.</summary>
        Task<MaintenanceTicket> CloseTicketAsync(int id, string resolution, decimal? cost, CancellationToken ct = default);

        Task<int> GetOpenTicketCountAsync(CancellationToken ct = default);

        /// <summary>Number of tickets per materiel — used by health-scoring.</summary>
        Task<IReadOnlyDictionary<int, int>> GetTicketCountPerMaterielAsync(CancellationToken ct = default);
    }
}
