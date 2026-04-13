using ITStockM.Domain.Entities;

namespace ITStockM.Services.Interfaces
{
    /// <summary>
    /// Manages the stage lifecycle of every IT asset.
    /// </summary>
    public interface IAssetLifecycleService
    {
        /// <summary>Returns the full stage-transition history for one asset.</summary>
        Task<IEnumerable<AssetLifecycleRecord>> GetLifecycleHistoryAsync(int materielId, CancellationToken ct = default);

        /// <summary>
        /// Closes the current open record and writes a new one with <paramref name="newStage"/>.
        /// Also updates <c>Materiel.LifecycleStatus</c>.
        /// </summary>
        Task<AssetLifecycleRecord> TransitionStageAsync(int materielId, string newStage, string? notes = null, CancellationToken ct = default);

        /// <summary>Returns all materiels whose current stage matches <paramref name="stage"/>.</summary>
        Task<IEnumerable<Materiel>> GetAssetsByStageAsync(string stage, CancellationToken ct = default);

        /// <summary>Assets whose warranty or estimated lifetime expire within the next <paramref name="withinMonths"/> months.</summary>
        Task<IEnumerable<Materiel>> GetAssetsNearingEndOfLifeAsync(int withinMonths = 6, CancellationToken ct = default);

        /// <summary>Returns all currently-open lifecycle records (EndDate == null).</summary>
        Task<IEnumerable<AssetLifecycleRecord>> GetCurrentStagesAsync(CancellationToken ct = default);
    }
}
