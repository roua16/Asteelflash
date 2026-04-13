using ITStockM.Data;
using ITStockM.Domain.Entities;
using ITStockM.Domain.Enums;
using ITStockM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITStockM.Services.Maintenance
{
    /// <summary>
    /// CRUD and workflow for maintenance tickets.
    /// Follows Open/Closed: new statuses or sub-workflows can be added without modifying this class.
    /// </summary>
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ITStockManagmentContext _context;
        private readonly IAssetLifecycleService _lifecycleService;
        private readonly ILogger<MaintenanceService> _logger;

        public MaintenanceService(
            ITStockManagmentContext context,
            IAssetLifecycleService lifecycleService,
            ILogger<MaintenanceService> logger)
        {
            _context          = context;
            _lifecycleService = lifecycleService;
            _logger           = logger;
        }

        public async Task<IEnumerable<MaintenanceTicket>> GetAllTicketsAsync(CancellationToken ct = default)
            => await _context.MaintenanceTickets
                             .Include(t => t.Materiel)
                             .Include(t => t.ReportedBy)
                             .OrderByDescending(t => t.ReportedAt)
                             .ToListAsync(ct);

        public async Task<IEnumerable<MaintenanceTicket>> GetTicketsByMaterielAsync(int materielId, CancellationToken ct = default)
            => await _context.MaintenanceTickets
                             .Where(t => t.MaterielId == materielId)
                             .OrderByDescending(t => t.ReportedAt)
                             .ToListAsync(ct);

        public async Task<MaintenanceTicket?> GetTicketByIdAsync(int id, CancellationToken ct = default)
            => await _context.MaintenanceTickets
                             .Include(t => t.Materiel)
                             .Include(t => t.ReportedBy)
                             .FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<MaintenanceTicket> CreateTicketAsync(MaintenanceTicket ticket, CancellationToken ct = default)
        {
            ticket.Status     = MaintenanceTicketStatus.Open;
            ticket.ReportedAt = DateTime.UtcNow;

            _context.MaintenanceTickets.Add(ticket);
            await _context.SaveChangesAsync(ct);

            // Transition the asset into "UnderMaintenance"
            await _lifecycleService.TransitionStageAsync(
                ticket.MaterielId,
                LifecycleStage.UnderMaintenance,
                $"Ticket #{ticket.Id} opened",
                ct);

            _logger.LogInformation("Maintenance ticket #{Id} created for MaterielId={MaterielId}", ticket.Id, ticket.MaterielId);
            return ticket;
        }

        public async Task<MaintenanceTicket> UpdateTicketAsync(int id, MaintenanceTicket ticket, CancellationToken ct = default)
        {
            var existing = await _context.MaintenanceTickets.FindAsync(new object[] { id }, ct)
                ?? throw new KeyNotFoundException($"Ticket {id} not found");

            existing.ProblemDescription    = ticket.ProblemDescription;
            existing.ReportedByEmployeeId  = ticket.ReportedByEmployeeId;
            existing.Status                = ticket.Status;
            existing.Cost                  = ticket.Cost;
            existing.Resolution            = ticket.Resolution;
            if (ticket.ResolvedAt.HasValue) existing.ResolvedAt = ticket.ResolvedAt;

            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Maintenance ticket #{Id} updated", id);
            return existing;
        }

        public async Task<MaintenanceTicket> CloseTicketAsync(int id, string resolution, decimal? cost, CancellationToken ct = default)
        {
            var ticket = await _context.MaintenanceTickets.FindAsync(new object[] { id }, ct)
                ?? throw new KeyNotFoundException($"Ticket {id} not found");

            ticket.Status     = MaintenanceTicketStatus.Closed;
            ticket.Resolution = resolution;
            ticket.Cost       = cost;
            ticket.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            // Return the asset to InStock after repair
            await _lifecycleService.TransitionStageAsync(
                ticket.MaterielId,
                LifecycleStage.InStock,
                $"Ticket #{ticket.Id} closed: {resolution}",
                ct);

            _logger.LogInformation("Maintenance ticket #{Id} closed", id);
            return ticket;
        }

        public Task<int> GetOpenTicketCountAsync(CancellationToken ct = default)
            => _context.MaintenanceTickets.CountAsync(t => t.Status == MaintenanceTicketStatus.Open, ct);

        public async Task<IReadOnlyDictionary<int, int>> GetTicketCountPerMaterielAsync(CancellationToken ct = default)
        {
            var dict = await _context.MaintenanceTickets
                .GroupBy(t => t.MaterielId)
                .Select(g => new { MaterielId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.MaterielId, x => x.Count, ct);
            return dict;
        }
    }
}
