using ITStockM.Data;
using ITStockM.Models.ITStockManagment;
using ITStockM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITStockM.Services.AssetLifecycle
{
    /// <summary>
    /// Manages lifecycle-stage transitions for every IT asset.
    /// Writes history records and keeps <c>Materiel.LifecycleStatus</c> in sync.
    /// </summary>
    public class AssetLifecycleService : IAssetLifecycleService
    {
        private readonly ITStockManagmentContext _context;
        private readonly ILogger<AssetLifecycleService> _logger;

        public AssetLifecycleService(
            ITStockManagmentContext context,
            ILogger<AssetLifecycleService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<IEnumerable<AssetLifecycleRecord>> GetLifecycleHistoryAsync(int materielId, CancellationToken ct = default)
            => await _context.AssetLifecycleRecords
                             .Where(r => r.MaterielId == materielId)
                             .OrderByDescending(r => r.StartDate)
                             .ToListAsync(ct);

        public async Task<AssetLifecycleRecord> TransitionStageAsync(int materielId, string newStage, string? notes = null, CancellationToken ct = default)
        {
            var materiel = await _context.Materiels.FindAsync(new object[] { materielId }, ct)
                ?? throw new KeyNotFoundException($"Materiel {materielId} not found");

            // Close the currently-open record
            var open = await _context.AssetLifecycleRecords
                                     .Where(r => r.MaterielId == materielId && r.EndDate == null)
                                     .FirstOrDefaultAsync(ct);
            if (open is not null)
                open.EndDate = DateTime.UtcNow;

            // Create the new record
            var record = new AssetLifecycleRecord
            {
                MaterielId = materielId,
                Stage      = newStage,
                StartDate  = DateTime.UtcNow,
                Notes      = notes
            };
            _context.AssetLifecycleRecords.Add(record);

            // Keep the denormalised status on the materiel in sync
            materiel.LifecycleStatus = newStage;

            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Asset {MaterielId} transitioned to stage '{Stage}'", materielId, newStage);
            return record;
        }

        public async Task<IEnumerable<Materiel>> GetAssetsByStageAsync(string stage, CancellationToken ct = default)
            => await _context.Materiels
                             .Where(m => m.LifecycleStatus == stage)
                             .ToListAsync(ct);

        public async Task<IEnumerable<Materiel>> GetAssetsNearingEndOfLifeAsync(int withinMonths = 6, CancellationToken ct = default)
        {
            var cutoff = DateTime.UtcNow.AddMonths(withinMonths);
            return await _context.Materiels
                .Where(m =>
                    m.LifecycleStatus != Models.Enums.LifecycleStage.Retired &&
                    (
                        // warranty expiring
                        m.Warranty <= cutoff ||
                        // or expected end-of-life reached
                        (m.PurchaseDate != null && m.ExpectedLifetimeMonths != null &&
                         m.PurchaseDate.Value.AddMonths(m.ExpectedLifetimeMonths.Value) <= cutoff)
                    ))
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<AssetLifecycleRecord>> GetCurrentStagesAsync(CancellationToken ct = default)
            => await _context.AssetLifecycleRecords
                             .Where(r => r.EndDate == null)
                             .Include(r => r.Materiel)
                             .ToListAsync(ct);
    }
}
