using System.Text.Json;
using FluentAssertions;
using ITStockM.Application.Common.Models;
using ITStockM.Application.Features.Maintenance.DTOs;
using ITStockM.Application.Features.Recommendations.DTOs;
using ITStockM.Services.Interfaces;
using ITStockM.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ITStockM.Tests.Api;

public sealed class HealthControllerIntegrationTests
{
    [Fact]
    public async Task GetAiModelMetrics_ReturnsExpectedResponseShape()
    {
        // Arrange
        var hardwareStatus = new AiModelStatusDto(
            ModelName: "hardware-recommendation",
            ActiveVersion: "20260530010000",
            PreviousVersion: "20260529010000",
            LastTrainedUtc: DateTime.UtcNow.AddMinutes(-30),
            RetrainIntervalMinutes: 240,
            TrainingSampleCount: 120,
            ValidationMetric: 0.82,
            DriftScore: 28,
            DriftLevel: "low",
            LastRetrainStatus: "activated",
            ActiveModelPath: "ml-models/hardware/model_20260530010000.zip");

        var ticketStatus = new AiModelStatusDto(
            ModelName: "ticket-prioritization",
            ActiveVersion: "20260530020000",
            PreviousVersion: "20260529020000",
            LastTrainedUtc: DateTime.UtcNow.AddMinutes(-45),
            RetrainIntervalMinutes: 240,
            TrainingSampleCount: 220,
            ValidationMetric: 0.79,
            DriftScore: 41,
            DriftLevel: "medium",
            LastRetrainStatus: "activated",
            ActiveModelPath: "ml-models/tickets/model_20260530020000.zip");

        var controller = new HealthController(
            new FakeHardwareRecommendationService(hardwareStatus),
            new FakeTicketPrioritizationService(ticketStatus));

        // Act
        var result = await controller.GetAiModelMetrics();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<AiModelMetricsSnapshotDto>().Subject;
        payload.Models.Should().HaveCount(2);
        payload.MaxDriftScore.Should().Be(41);
        payload.FleetDriftLevel.Should().Be("medium");

        var json = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        root.TryGetProperty("generatedAtUtc", out _).Should().BeTrue();
        root.TryGetProperty("models", out var modelsEl).Should().BeTrue();
        root.TryGetProperty("maxDriftScore", out _).Should().BeTrue();
        root.TryGetProperty("fleetDriftLevel", out _).Should().BeTrue();

        modelsEl.ValueKind.Should().Be(JsonValueKind.Array);
        modelsEl.GetArrayLength().Should().Be(2);

        var firstModel = modelsEl.EnumerateArray().First();
        firstModel.TryGetProperty("modelName", out _).Should().BeTrue();
        firstModel.TryGetProperty("activeVersion", out _).Should().BeTrue();
        firstModel.TryGetProperty("previousVersion", out _).Should().BeTrue();
        firstModel.TryGetProperty("lastTrainedUtc", out _).Should().BeTrue();
        firstModel.TryGetProperty("retrainIntervalMinutes", out _).Should().BeTrue();
        firstModel.TryGetProperty("trainingSampleCount", out _).Should().BeTrue();
        firstModel.TryGetProperty("validationMetric", out _).Should().BeTrue();
        firstModel.TryGetProperty("driftScore", out _).Should().BeTrue();
        firstModel.TryGetProperty("driftLevel", out _).Should().BeTrue();
        firstModel.TryGetProperty("lastRetrainStatus", out _).Should().BeTrue();
        firstModel.TryGetProperty("activeModelPath", out _).Should().BeTrue();
    }

    private sealed class FakeHardwareRecommendationService : IHardwareRecommendationService
    {
        private readonly AiModelStatusDto _status;

        public FakeHardwareRecommendationService(AiModelStatusDto status)
        {
            _status = status;
        }

        public Task<IReadOnlyList<HardwareRecommendationDto>> RecommendAsync(HardwareRecommendationRequestDto request, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<HardwareRecommendationDto>>(Array.Empty<HardwareRecommendationDto>());

        public Task RetrainAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task<AiModelStatusDto> GetModelStatusAsync(CancellationToken ct = default)
            => Task.FromResult(_status);
    }

    private sealed class FakeTicketPrioritizationService : ITicketPrioritizationService
    {
        private readonly AiModelStatusDto _status;

        public FakeTicketPrioritizationService(AiModelStatusDto status)
        {
            _status = status;
        }

        public Task<TicketPriorityPredictionDto> PredictPriorityAsync(TicketPriorityRequestDto request, CancellationToken ct = default)
            => Task.FromResult(new TicketPriorityPredictionDto("Moyenne", 50m, new List<string>(), "stub"));

        public Task RetrainAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task<AiModelStatusDto> GetModelStatusAsync(CancellationToken ct = default)
            => Task.FromResult(_status);
    }
}
