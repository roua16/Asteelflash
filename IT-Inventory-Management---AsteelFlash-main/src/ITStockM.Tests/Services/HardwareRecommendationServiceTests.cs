using FluentAssertions;
using ITStockM.Application.Features.Recommendations.DTOs;
using ITStockM.Data;
using ITStockM.Domain.Entities;
using ITStockM.Services.Recommendations;
using Microsoft.EntityFrameworkCore;

namespace ITStockM.Tests.Services;

public class HardwareRecommendationServiceTests
{
    private static ITStockManagmentContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ITStockManagmentContext>()
            .UseInMemoryDatabase($"reco_{Guid.NewGuid()}")
            .Options;

        return new ITStockManagmentContext(options);
    }

    [Fact]
    public async Task RecommendAsync_DeveloperProfile_PrefersHighPerformanceAsset()
    {
        await using var context = CreateContext();

        context.Materiels.AddRange(
            new Materiel
            {
                MaterielName = "Laptop i7 16GB",
                Type = "Laptop",
                QuantityITStock = 4,
                QuantityPDRStock = 0,
                IrreparableQuantity = 0,
                Repairing_Quantity = 0,
                Warranty = DateTime.UtcNow.AddYears(2),
                LifecycleStatus = "InStock",
                CurrentHealthScore = 92m
            },
            new Materiel
            {
                MaterielName = "Office PC standard",
                Type = "Desktop",
                QuantityITStock = 8,
                QuantityPDRStock = 0,
                IrreparableQuantity = 0,
                Repairing_Quantity = 0,
                Warranty = DateTime.UtcNow.AddYears(2),
                LifecycleStatus = "InStock",
                CurrentHealthScore = 88m
            });

        await context.SaveChangesAsync();

        var tempModelRoot = Path.Combine(Path.GetTempPath(), "itstockm-models", "test-hardware-recommendation");
        var mlContext = new Microsoft.ML.MLContext(42);
        var aiArtifactManager = new ITStockM.Services.AiModelArtifactManager(
            mlContext,
            modelRootDirectory: tempModelRoot,
            scopedModelDirectoryName: "hardware-recommendation",
            maxVersionsToKeep: 2);

        var artifactManager = new ITStockM.Services.HardwareRecommendationArtifactManager(aiArtifactManager);

        var service = new HardwareRecommendationService(
            context,
            artifactManager,
            options: null);
        var result = await service.RecommendAsync(new HardwareRecommendationRequestDto(
            Role: "Developer",
            Service: "IT",
            UsageLevel: 5,
            NeedsHighPerformance: true,
            NeedsGraphics: false,
            PreferredType: "Laptop",
            TopN: 2));

        result.Should().HaveCount(2);
        result[0].MaterielName.Should().Contain("i7");
        result[0].MatchScore.Should().BeGreaterThan(result[1].MatchScore);
    }
}
