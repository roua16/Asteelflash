using FluentAssertions;
using ITStockM.Data;
using ITStockM.Domain.Enums;
using ITStockM.Services.Prediction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ITStockM.Tests.Services
{
    /// <summary>
    /// Unit tests for the health-score formula in PredictionService.
    /// ComputeHealthScore is a pure function, so no database is required.
    /// </summary>
    public class PredictionServiceTests
    {
        private static PredictionService CreateService()
        {
            var options = new DbContextOptionsBuilder<ITStockManagmentContext>()
                .UseInMemoryDatabase($"pred_{Guid.NewGuid()}")
                .Options;
            var ctx = new ITStockManagmentContext(options);
            return new PredictionService(ctx, NullLogger<PredictionService>.Instance);
        }

        // ─── Perfect asset ────────────────────────────────────────────────────────

        [Fact]
        public void ComputeHealthScore_NewAsset_NoIssues_Returns100()
        {
            var svc = CreateService();
            var score = svc.ComputeHealthScore(
                ageMonths:               0,
                expectedLifetimeMonths: 48,
                totalAssignments:        0,
                closedTickets:           0,
                openTickets:             0);

            score.Should().Be(100m);
        }

        // ─── Fully degraded asset ─────────────────────────────────────────────────

        [Fact]
        public void ComputeHealthScore_FullyAged_MaxUsage_MaxFailures_Returns0()
        {
            var svc = CreateService();
            var score = svc.ComputeHealthScore(
                ageMonths:               48,  // = expected lifetime  → ageFactor  = 1
                expectedLifetimeMonths:  48,
                totalAssignments:        30,  // normalisation ceiling → usageFactor = 1
                closedTickets:            5,  // normalisation ceiling → failureFactor = 1
                openTickets:              3); // normalisation ceiling → maintenanceFactor = 1

            // 100 − 25 − 25 − 30 − 20 = 0
            score.Should().Be(0m);
        }

        // ─── Half-life asset, moderate usage, no failures ─────────────────────────

        [Fact]
        public void ComputeHealthScore_HalfLife_ModerateUsage_Returns_Expected()
        {
            var svc = CreateService();
            // ageFactor = 24/48 = 0.5  → penalty = 25 × 0.5 = 12.5
            // usageFactor = 15/30 = 0.5 → penalty = 25 × 0.5 = 12.5
            // failureFactor = 0         → penalty = 0
            // maintenanceFactor = 0     → penalty = 0
            // expected = 100 − 12.5 − 12.5 = 75
            var score = svc.ComputeHealthScore(24, 48, 15, 0, 0);

            score.Should().Be(75m);
        }

        // ─── Age beyond expected lifetime (clamping) ─────────────────────────────

        [Fact]
        public void ComputeHealthScore_AgeBeyondExpected_ClampsAgeFactor()
        {
            var svc = CreateService();
            // ageMonths (96) > expectedLifetimeMonths (48), ageFactor clamped to 1
            var scoreClamped = svc.ComputeHealthScore(96, 48, 0, 0, 0);
            var scoreAtMax   = svc.ComputeHealthScore(48, 48, 0, 0, 0);

            scoreClamped.Should().Be(scoreAtMax, "age factor must be clamped at 1.0");
        }

        // ─── Assignments beyond ceiling (clamping) ────────────────────────────────

        [Fact]
        public void ComputeHealthScore_AssignmentsAboveCeiling_ClampsUsageFactor()
        {
            var svc = CreateService();
            var score60  = svc.ComputeHealthScore(0, 48, 60, 0, 0); // 60 > 30 ceiling
            var scoreMed = svc.ComputeHealthScore(0, 48, 30, 0, 0); // exactly at ceiling

            score60.Should().Be(scoreMed, "usage factor must be clamped at 1.0");
        }

        // ─── HealthStatus.FromScore factory ──────────────────────────────────────

        [Theory]
        [InlineData(100, "Good")]
        [InlineData(75,  "Good")]
        [InlineData(74,  "Fair")]
        [InlineData(45,  "Fair")]
        [InlineData(44,  "Poor")]
        [InlineData(0,   "Poor")]
        public void HealthStatus_FromScore_ReturnsCorrectBand(decimal score, string expected)
        {
            HealthStatus.FromScore(score).Should().Be(expected);
        }

        // ─── Score is always within [0, 100] ─────────────────────────────────────

        [Theory]
        [InlineData(0,   48,  0,  0,  0)]
        [InlineData(48,  48, 30,  5,  3)]
        [InlineData(24,  48, 15,  2,  1)]
        [InlineData(120, 48, 60, 10, 10)] // all beyond ceilings
        public void ComputeHealthScore_AlwaysWithin0And100(int age, int lifetime, int assigns, int closed, int open)
        {
            var svc   = CreateService();
            var score = svc.ComputeHealthScore(age, lifetime, assigns, closed, open);

            score.Should().BeGreaterThanOrEqualTo(0m).And.BeLessThanOrEqualTo(100m);
        }
    }
}
