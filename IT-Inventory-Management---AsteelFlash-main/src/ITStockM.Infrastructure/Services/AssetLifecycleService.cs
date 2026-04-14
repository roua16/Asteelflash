using ITStockM.Domain.Entities;
using ITStockM.Domain.Enums;
using ITStockM.Repositories;
using ITStockM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITStockM.Services.AssetLifecycle
{
    /// <summary>
    /// Manages lifecycle-stage transitions for every IT asset.
    /// Refactored to use repository injection for better testability and separation of concerns.
    /// Writes history records and keeps <c>Materiel.LifecycleStatus</c> in sync.
    /// </summary>
    public class AssetLifecycleService : IAssetLifecycleService
    {
        private readonly IRepository<AssetLifecycleRecord> _lifecycleRecordRepository;
        private readonly IRepository<Materiel> _materielRepository;
        private readonly ILogger<AssetLifecycleService> _logger;

        public AssetLifecycleService(
            IRepository<AssetLifecycleRecord> lifecycleRecordRepository,
            IRepository<Materiel> materielRepository,
            ILogger<AssetLifecycleService> logger)
        {
            _lifecycleRecordRepository = lifecycleRecordRepository;
            _materielRepository = materielRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<AssetLifecycleRecord>> GetLifecycleHistoryAsync(int materielId, CancellationToken ct = default)
        {
            return await _lifecycleRecordRepository.Query()
                .Where(r => r.MaterielId == materielId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync(ct);
        }

        public async Task<AssetLifecycleRecord> TransitionStageAsync(int materielId, string newStage, string? notes = null, CancellationToken ct = default)
        {
            var materiel = await _materielRepository.GetByIdAsync(materielId, ct)
                ?? throw new KeyNotFoundException($"Materiel {materielId} not found");

            // Close the currently-open record
            var open = await _lifecycleRecordRepository.Query()
                .Where(r => r.MaterielId == materielId && r.EndDate == null)
                .FirstOrDefaultAsync(ct);
            if (open is not null)
                open.EndDate = DateTime.UtcNow;

            // Create the new record
            var record = new AssetLifecycleRecord
            {
                MaterielId = materielId,
                Stage = newStage,
                StartDate = DateTime.UtcNow,
                Notes = notes
            };
            await _lifecycleRecordRepository.AddAsync(record);

            // Keep the denormalised status on the materiel in sync
            materiel.LifecycleStatus = newStage;
            _materielRepository.Update(materiel);

            await _lifecycleRecordRepository.SaveChangesAsync();
            _logger.LogInformation("Asset {MaterielId} transitioned to stage '{Stage}'", materielId, newStage);
            return record;
        }

        public async Task<IEnumerable<Materiel>> GetAssetsByStageAsync(string stage, CancellationToken ct = default)
        {
            return await _materielRepository.Query()
                .Where(m => m.LifecycleStatus == stage)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Materiel>> GetAssetsNearingEndOfLifeAsync(int withinMonths = 6, CancellationToken ct = default)
        {
            var cutoff = DateTime.UtcNow.AddMonths(withinMonths);
            return await _materielRepository.Query()
                .Where(m =>
                    m.LifecycleStatus != LifecycleStage.Retired &&
                    (
                        m.Warranty <= cutoff ||
                        (m.PurchaseDate != null && m.ExpectedLifetimeMonths != null &&
                         m.PurchaseDate.Value.AddMonths(m.ExpectedLifetimeMonths.Value) <= cutoff)
                    ))
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<AssetLifecycleRecord>> GetCurrentStagesAsync(CancellationToken ct = default)
        {
            return await _lifecycleRecordRepository.Query()
                .Where(r => r.EndDate == null)
                .Include(r => r.Materiel)
                .ToListAsync(ct);
        }
    }
}
