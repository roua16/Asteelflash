using ITStockM.Domain.Entities;

namespace ITStockM.Services.Interfaces
{
    /// <summary>
    /// Computes asset health scores and replacement predictions.
    /// </summary>
    public interface IPredictionService
    {
        /// <summary>
        /// Computes and persists a prediction for a single asset.
        /// Overwrites any previous prediction for the same <paramref name="materielId"/>.
        /// </summary>
        Task<AssetPrediction> CalculateAndSavePredictionAsync(int materielId, CancellationToken ct = default);

        /// <summary>Recalculates and persists predictions for ALL assets. Called daily by the background service.</summary>
        Task<IEnumerable<AssetPrediction>> RecalculateAllPredictionsAsync(CancellationToken ct = default);

        /// <summary>Returns the latest persisted prediction for each asset.</summary>
        Task<IEnumerable<AssetPrediction>> GetLatestPredictionsAsync(CancellationToken ct = default);

        /// <summary>Assets whose health score is below <paramref name="maxHealthScore"/> (default 50 = at risk).</summary>
        Task<IEnumerable<AssetPrediction>> GetHighRiskAssetsAsync(decimal maxHealthScore = 50m, CancellationToken ct = default);

        /// <summary>
        /// Pure, side-effect-free computation — testable without a database.
        /// </summary>
        /// <param name="ageMonths">Current age in months.</param>
        /// <param name="expectedLifetimeMonths">Expected lifetime in months.</param>
        /// <param name="totalAssignments">Total number of assignment records linked to the asset.</param>
        /// <param name="closedTickets">Number of closed (repaired) maintenance tickets.</param>
        /// <param name="openTickets">Number of currently-open maintenance tickets.</param>
        decimal ComputeHealthScore(int ageMonths, int expectedLifetimeMonths, int totalAssignments, int closedTickets, int openTickets);
    }
}
