using FluentAssertions;
using ITStockM.Data;
using ITStockM.Domain.Enums;
using ITStockM.Domain.Entities;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ITStockM.Tests.Services
{
    /// <summary>
    /// Unit tests for MaintenanceService using an EF Core InMemory database.
    /// IAssetLifecycleService is mocked to isolate the maintenance workflow.
    /// </summary>
    public class MaintenanceServiceTests
    {
        private static ITStockManagmentContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ITStockManagmentContext>()
                .UseInMemoryDatabase($"maint_{Guid.NewGuid()}")
                .Options;
            return new ITStockManagmentContext(options);
        }

        private static (MaintenanceService svc, Mock<IAssetLifecycleService> lifecycleMock) CreateService(
            ITStockManagmentContext context)
        {
            var mock = new Mock<IAssetLifecycleService>();
            mock.Setup(s => s.TransitionStageAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AssetLifecycleRecord());

            var svc = new MaintenanceService(context, mock.Object, NullLogger<MaintenanceService>.Instance);
            return (svc, mock);
        }

        private static async Task<Materiel> SeedMaterielAsync(ITStockManagmentContext ctx)
        {
            var m = new Materiel
            {
                MaterielName       = "Test Laptop",
                SerialNumber       = "SN-001",
                Type               = "IT",
                QuantityITStock    = 1,
                QuantityPDRStock   = 0,
                IrreparableQuantity= 0,
                Repairing_Quantity = 0,
                Warranty           = DateTime.UtcNow.AddYears(1),
                LifecycleStatus    = "InStock"
            };
            ctx.Materiels.Add(m);
            await ctx.SaveChangesAsync();
            return m;
        }

        // ─── CreateTicketAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task CreateTicketAsync_PersistsTicket_WithOpenStatus()
        {
            var ctx = CreateContext();
            var materiel = await SeedMaterielAsync(ctx);
            var (svc, _) = CreateService(ctx);

            var ticket = await svc.CreateTicketAsync(new MaintenanceTicket
            {
                MaterielId         = materiel.Id,
                ProblemDescription = "Screen broken"
            });

            ticket.Id.Should().BeGreaterThan(0);
            ticket.Status.Should().Be(MaintenanceTicketStatus.Open);
            ticket.ReportedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CreateTicketAsync_TransitionsAsset_ToUnderMaintenance()
        {
            var ctx = CreateContext();
            var materiel = await SeedMaterielAsync(ctx);
            var (svc, lifecycleMock) = CreateService(ctx);

            await svc.CreateTicketAsync(new MaintenanceTicket
            {
                MaterielId         = materiel.Id,
                ProblemDescription = "Fan noisy"
            });

            lifecycleMock.Verify(l => l.TransitionStageAsync(
                materiel.Id,
                LifecycleStage.UnderMaintenance,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ─── GetAllTicketsAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task GetAllTicketsAsync_ReturnsAllTickets_InDescendingOrder()
        {
            var ctx = CreateContext();
            var materiel = await SeedMaterielAsync(ctx);
            var (svc, _) = CreateService(ctx);

            await svc.CreateTicketAsync(new MaintenanceTicket { MaterielId = materiel.Id, ProblemDescription = "First" });
            await Task.Delay(10);
            await svc.CreateTicketAsync(new MaintenanceTicket { MaterielId = materiel.Id, ProblemDescription = "Second" });

            var tickets = (await svc.GetAllTicketsAsync()).ToList();

            tickets.Should().HaveCount(2);
            tickets[0].ProblemDescription.Should().Be("Second", "most recent first");
        }

        // ─── CloseTicketAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task CloseTicketAsync_SetsStatus_ResolutionAndCost()
        {
            var ctx = CreateContext();
            var materiel = await SeedMaterielAsync(ctx);
            var (svc, _) = CreateService(ctx);

            var ticket = await svc.CreateTicketAsync(new MaintenanceTicket
            {
                MaterielId         = materiel.Id,
                ProblemDescription = "Hard drive failed"
            });

            var closed = await svc.CloseTicketAsync(ticket.Id, "Replaced HDD", 120.50m);

            closed.Status.Should().Be(MaintenanceTicketStatus.Closed);
            closed.Resolution.Should().Be("Replaced HDD");
            closed.Cost.Should().Be(120.50m);
            closed.ResolvedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task CloseTicketAsync_TransitionsAsset_BackToInStock()
        {
            var ctx = CreateContext();
            var materiel = await SeedMaterielAsync(ctx);
            var (svc, lifecycleMock) = CreateService(ctx);

            var ticket = await svc.CreateTicketAsync(new MaintenanceTicket
            {
                MaterielId         = materiel.Id,
                ProblemDescription = "RAM failure"
            });

            await svc.CloseTicketAsync(ticket.Id, "RAM replaced", 50m);

            lifecycleMock.Verify(l => l.TransitionStageAsync(
                materiel.Id,
                LifecycleStage.InStock,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CloseTicketAsync_ThrowsKeyNotFound_WhenTicketDoesNotExist()
        {
            var ctx = CreateContext();
            var (svc, _) = CreateService(ctx);

            Func<Task> act = () => svc.CloseTicketAsync(9999, "n/a", null);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        // ─── GetOpenTicketCountAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetOpenTicketCountAsync_ReturnsOnlyOpenTickets()
        {
            var ctx = CreateContext();
            var materiel = await SeedMaterielAsync(ctx);
            var (svc, _) = CreateService(ctx);

            var t1 = await svc.CreateTicketAsync(new MaintenanceTicket { MaterielId = materiel.Id, ProblemDescription = "A" });
            var t2 = await svc.CreateTicketAsync(new MaintenanceTicket { MaterielId = materiel.Id, ProblemDescription = "B" });
            await svc.CloseTicketAsync(t1.Id, "Fixed", null);

            var count = await svc.GetOpenTicketCountAsync();

            count.Should().Be(1);
        }
    }
}
