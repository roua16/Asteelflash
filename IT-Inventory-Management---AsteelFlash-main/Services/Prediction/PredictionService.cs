using ITStockM.Data;
using ITStockM.Models.ITStockManagment;
using ITStockM.Models.Enums;
using ITStockM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITStockM.Services.Prediction
{
    /// <summary>
    /// Computes asset health scores and generates predictive replacement recommendations.
    ///
    /// Health score formula (0–100, higher = healthier):
    ///   score = 100
    ///         − 25 × ageFactor        (age relative to expected lifetime)
    ///         − 25 × usageFactor      (frequency of assignments)
    ///         − 30 × failureFactor    (number of closed repair tickets)
    ///         − 20 × maintenanceFactor(number of open tickets right now)
    ///
    /// All factors are clamped to [0, 1] before weighting.
    /// </summary>
    public class PredictionService : IPredictionService
    {
        // ─── Scoring weights ─────────────────────────────────────────────────────
        private const decimal AgeWeight         = 0.25m;
        private const decimal UsageWeight       = 0.25m;
        private const decimal FailureWeight     = 0.30m;
        private const decimal MaintenanceWeight = 0.20m;

        // ─── Normalisation ceilings ──────────────────────────────────────────────
        private const int MaxNormalisedAssignments = 30;  // 30+ assignments treated as maximum usage
        private const int MaxNormalisedFailures    =  5;  // 5+ closed tickets → max failure penalty
        private const int MaxNormalisedOpenTickets =  3;  // 3+ open tickets → max active-maintenance penalty

        private readonly ITStockManagmentContext _context;
        private readonly ILogger<PredictionService> _logger;

        public PredictionService(
            ITStockManagmentContext context,
            ILogger<PredictionService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        // ─── IPredictionService ──────────────────────────────────────────────────

        public async Task<AssetPrediction> CalculateAndSavePredictionAsync(int materielId, CancellationToken ct = default)
        {
            var materiel = await _context.Materiels
                .Include(m => m.AssignmentMateriels)
                .Include(m => m.MaintenanceTickets)
                .FirstOrDefaultAsync(m => m.Id == materielId, ct)
                ?? throw new KeyNotFoundException($"Materiel {materielId} not found");

            var prediction = BuildPrediction(materiel);

            // Upsert: replace previous prediction for the same asset
            var existing = await _context.AssetPredictions
                .FirstOrDefaultAsync(p => p.MaterielId == materielId, ct);

            if (existing is null)
                _context.AssetPredictions.Add(prediction);
            else
            {
                existing.CalculatedAt              = prediction.CalculatedAt;
                existing.HealthScore               = prediction.HealthScore;
                existing.HealthStatus              = prediction.HealthStatus;
                existing.PredictedFailureDate      = prediction.PredictedFailureDate;
                existing.RecommendedReplacementDate= prediction.RecommendedReplacementDate;
                existing.RecommendationReason      = prediction.RecommendationReason;
                existing.EstimatedReplacementCost  = prediction.EstimatedReplacementCost;
                prediction = existing;
            }

            // Keep denormalised health score on Materiel
            materiel.CurrentHealthScore = prediction.HealthScore;

            await _context.SaveChangesAsync(ct);
            return prediction;
        }

        public async Task<IEnumerable<AssetPrediction>> RecalculateAllPredictionsAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Starting daily health-score recalculation for all assets");

            var materiels = await _context.Materiels
                .Where(m => m.LifecycleStatus != LifecycleStage.Retired)
                .Include(m => m.AssignmentMateriels)
                .Include(m => m.MaintenanceTickets)
                .ToListAsync(ct);

            var results = new List<AssetPrediction>(materiels.Count);
            foreach (var m in materiels)
            {
                try
                {
                    var pred = await CalculateAndSavePredictionAsync(m.Id, ct);
                    results.Add(pred);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to calculate prediction for MaterielId={Id}", m.Id);
                }
            }

            _logger.LogInformation("Recalculation complete: {Count} assets processed", results.Count);
            return results;
        }

        public async Task<IEnumerable<AssetPrediction>> GetLatestPredictionsAsync(CancellationToken ct = default)
            => await _context.AssetPredictions
                             .Include(p => p.Materiel)
                             .OrderByDescending(p => p.CalculatedAt)
                             .ToListAsync(ct);

        public async Task<IEnumerable<AssetPrediction>> GetHighRiskAssetsAsync(decimal maxHealthScore = 50m, CancellationToken ct = default)
            => await _context.AssetPredictions
                             .Include(p => p.Materiel)
                             .Where(p => p.HealthScore <= maxHealthScore)
                             .OrderBy(p => p.HealthScore)
                             .ToListAsync(ct);

        /// <inheritdoc/>
        public decimal ComputeHealthScore(int ageMonths, int expectedLifetimeMonths, int totalAssignments, int closedTickets, int openTickets)
        {
            var ageFactor    = Clamp((decimal)ageMonths         / Math.Max(expectedLifetimeMonths, 1));
            var usageFactor  = Clamp((decimal)totalAssignments  / MaxNormalisedAssignments);
            var failureFactor= Clamp((decimal)closedTickets     / MaxNormalisedFailures);
            var maintFactor  = Clamp((decimal)openTickets       / MaxNormalisedOpenTickets);

            var score = 100m
                - AgeWeight         * ageFactor     * 100m
                - UsageWeight       * usageFactor   * 100m
                - FailureWeight     * failureFactor * 100m
                - MaintenanceWeight * maintFactor   * 100m;

            return Math.Round(Math.Max(0m, Math.Min(100m, score)), 2);
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private AssetPrediction BuildPrediction(Materiel m)
        {
            var now              = DateTime.UtcNow;
            var purchaseDate     = m.PurchaseDate ?? now.AddMonths(-12); // assume 1 year old if unknown
            var ageMonths        = (int)((now - purchaseDate).TotalDays / 30.44);
            var expectedMonths   = m.ExpectedLifetimeMonths ?? 48;
            var assignments      = m.AssignmentMateriels?.Count ?? 0;
            var closedTickets    = m.MaintenanceTickets?.Count(t => t.Status == Models.Enums.MaintenanceTicketStatus.Closed) ?? 0;
            var openTickets      = m.MaintenanceTickets?.Count(t => t.Status != Models.Enums.MaintenanceTicketStatus.Closed) ?? 0;

            var score      = ComputeHealthScore(ageMonths, expectedMonths, assignments, closedTickets, openTickets);
            var status     = HealthStatus.FromScore(score);
            var eolDate    = purchaseDate.AddMonths(expectedMonths);
            var remaining  = (int)((eolDate - now).TotalDays / 30.44);

            DateTime? predictedFailure    = null;
            DateTime? recommendedReplacement = null;
            string?   reason              = null;

            if (score < 75)
            {
                // Linear interpolation: healthier → more time left
                var monthsToFailure  = Math.Max(1, (int)(remaining * (score / 100m)));
                predictedFailure     = now.AddMonths(monthsToFailure);
                recommendedReplacement = now.AddMonths(Math.Max(1, monthsToFailure - 2)); // 2 months lead time

                reason = score < 45
                    ? $"Critical: {closedTickets} past failures, {openTickets} open tickets, {ageMonths} months old"
                    : $"Fair condition: asset is {ageMonths}/{expectedMonths} months through expected lifetime";
            }

            return new AssetPrediction
            {
                MaterielId               = m.Id,
                CalculatedAt             = now,
                HealthScore              = score,
                HealthStatus             = status,
                PredictedFailureDate     = predictedFailure,
                RecommendedReplacementDate = recommendedReplacement,
                RecommendationReason     = reason
            };
        }

        private static decimal Clamp(decimal value) => Math.Max(0m, Math.Min(1m, value));
    }
}
