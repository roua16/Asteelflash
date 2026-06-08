using FluentAssertions;
using ITStockM.Application.Features.Maintenance.DTOs;
using ITStockM.Services.Maintenance;

namespace ITStockM.Tests.Services;

public class TicketPrioritizationServiceTests
{
    [Fact]
    public async Task PredictPriorityAsync_CriticalSignals_ReturnsCritique()
    {
        var service = new TicketPrioritizationService();

        var result = await service.PredictPriorityAsync(new TicketPriorityRequestDto(
            Title: "Production server down",
            Description: "Main ERP server is inaccessible and users cannot work",
            Category: "Infrastructure",
            UrgencyLevel: 5,
            ImpactedUsers: 250,
            EquipmentCriticality: 5,
            EquipmentType: "Server"));

        result.Priority.Should().Be("Critique");
        result.PriorityScore.Should().BeGreaterThanOrEqualTo(80m);
        result.MatchedKeywords.Should().Contain("server");
    }

    [Fact]
    public async Task PredictPriorityAsync_MinorIssue_ReturnsFaible()
    {
        var service = new TicketPrioritizationService();

        var result = await service.PredictPriorityAsync(new TicketPriorityRequestDto(
            Title: "Printer slow",
            Description: "Office printer is a bit slow for one user",
            Category: "Peripheral",
            UrgencyLevel: 1,
            ImpactedUsers: 1,
            EquipmentCriticality: 1,
            EquipmentType: "Printer"));

        result.Priority.Should().Be("Faible");
        result.PriorityScore.Should().BeLessThan(35m);
    }
}
