using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using Moq;
using Xunit;

using ITStockM.Data;
using ITStockM.Domain.Entities;
using ITStockM.Repositories;
using ITStockM.Services;
using ITStockM.Services.Interfaces;

namespace ITStockM.Tests.Services
{
    public class ITStockManagmentTopUsedTests
    {
        private ITStockManagmentContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ITStockManagmentContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ITStockManagmentContext(options);
        }

        private class TestNav : NavigationManager
        {
            public TestNav()
            {
                Initialize("http://localhost/", "http://localhost/");
            }
            protected override void NavigateToCore(string uri, bool forceLoad) { }
        }

        [Fact]
        public async Task GetTopUsedMateriels_Returns_Top3_With_Correct_Share()
        {
            // Arrange
            var ctx = CreateInMemoryContext("topused_db_1");

            var m1 = new Materiel { Id = 1, MaterielName = "A", Type = "Consumable", QuantityITStock = 10, QuantityPDRStock = 0, Warranty = System.DateTime.UtcNow };
            var m2 = new Materiel { Id = 2, MaterielName = "B", Type = "Consumable", QuantityITStock = 5, QuantityPDRStock = 0, Warranty = System.DateTime.UtcNow };
            var m3 = new Materiel { Id = 3, MaterielName = "C", Type = "Consumable", QuantityITStock = 2, QuantityPDRStock = 0, Warranty = System.DateTime.UtcNow };
            var m4 = new Materiel { Id = 4, MaterielName = "D", Type = "Consumable", QuantityITStock = 1, QuantityPDRStock = 0, Warranty = System.DateTime.UtcNow };

            await ctx.Materiels.AddRangeAsync(m1, m2, m3, m4);

            // Assignment usage: A=5, B=2
            await ctx.AssignmentMateriels.AddRangeAsync(
                new AssignmentMateriel { MaterielId = 1, Qte = 5, AssignmentId = 1 },
                new AssignmentMateriel { MaterielId = 2, Qte = 2, AssignmentId = 1 }
            );

            // Delivery usage: A=3, C=7
            await ctx.DeliveryOrderMateriels.AddRangeAsync(
                new DeliveryOrderMateriel { MaterielId = 1, Qte = 3, DeliveryOrderNumber = "DO-1" },
                new DeliveryOrderMateriel { MaterielId = 3, Qte = 7, DeliveryOrderNumber = "DO-1" }
            );

            await ctx.SaveChangesAsync();

            var mockExport = new Mock<ITStockM.Services.Export.IExportService>();
            var mockAssignRepo = new Mock<IAssignmentRepository>();
            var mockReqRepo = new Mock<IRequestRepository>();
            var mockDeliveryRepo = new Mock<IDeliveryOrderRepository>();
            var mockMaterielRepo = new Mock<IMaterielRepository>();
            var mockAssignService = new Mock<global::ITStockM.Services.Assignments.IAssignmentService>();
            var mockReqService = new Mock<global::ITStockM.Services.Requests.IRequestService>();
            var mockDeliveryService = new Mock<global::ITStockM.Services.DeliveryOrders.IDeliveryOrderService>();
            var mockMaterielService = new Mock<global::ITStockM.Services.Materiels.IMaterielService>();

            var nav = new TestNav();

            var service = new ITStockManagmentService(ctx, null!, nav, mockExport.Object,
                mockAssignRepo.Object, mockReqRepo.Object, mockDeliveryRepo.Object, mockMaterielRepo.Object,
                mockAssignService.Object, mockReqService.Object, mockDeliveryService.Object, mockMaterielService.Object,
                null);

            // Act
            var top3 = await service.GetTopUsedMateriels(3);

            // Assert
            top3.Should().HaveCount(3);
            top3[0].Id.Should().Be(1); // A -> 5 + 3 = 8
            top3[0].UsageCount.Should().Be(8);
            top3[1].Id.Should().Be(3); // C -> 7
            top3[1].UsageCount.Should().Be(7);
            top3[2].Id.Should().Be(2); // B -> 2
            top3[2].UsageCount.Should().Be(2);

            // totalUsage = 8 + 7 + 2 = 17
            top3[0].UsageShare.Should().BeApproximately(8.0 / 17.0, 0.0001);
            top3[1].UsageShare.Should().BeApproximately(7.0 / 17.0, 0.0001);
            top3[2].UsageShare.Should().BeApproximately(2.0 / 17.0, 0.0001);
        }

        [Fact]
        public async Task SendTopUsedMaterielsEmailIfNotSentToday_Invokes_NotificationService_When_Top3Exists()
        {
            // Arrange
            var ctx = CreateInMemoryContext("topused_db_2");

            var m1 = new Materiel { Id = 1, MaterielName = "A", Type = "Consumable", QuantityITStock = 10, QuantityPDRStock = 0, Warranty = System.DateTime.UtcNow };
            await ctx.Materiels.AddAsync(m1);
            await ctx.AssignmentMateriels.AddAsync(new AssignmentMateriel { MaterielId = 1, Qte = 1, AssignmentId = 1 });
            await ctx.SaveChangesAsync();

            var mockExport = new Mock<ITStockM.Services.Export.IExportService>();
            var mockAssignRepo = new Mock<IAssignmentRepository>();
            var mockReqRepo = new Mock<IRequestRepository>();
            var mockDeliveryRepo = new Mock<IDeliveryOrderRepository>();
            var mockMaterielRepo = new Mock<IMaterielRepository>();
            var mockAssignService = new Mock<global::ITStockM.Services.Assignments.IAssignmentService>();
            var mockReqService = new Mock<global::ITStockM.Services.Requests.IRequestService>();
            var mockDeliveryService = new Mock<global::ITStockM.Services.DeliveryOrders.IDeliveryOrderService>();
            var mockMaterielService = new Mock<global::ITStockM.Services.Materiels.IMaterielService>();

            var mockOperationNotification = new Mock<IOperationNotificationService>();
            mockOperationNotification.Setup(n => n.NotifyTopUsedMateriels(It.IsAny<IEnumerable<ITStockM.Models.ViewModels.MaterielUsageViewModel>>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var nav = new TestNav();

            var service = new ITStockManagmentService(ctx, null!, nav, mockExport.Object,
                mockAssignRepo.Object, mockReqRepo.Object, mockDeliveryRepo.Object, mockMaterielRepo.Object,
                mockAssignService.Object, mockReqService.Object, mockDeliveryService.Object, mockMaterielService.Object,
                mockOperationNotification.Object);

            // Act
            await service.SendTopUsedMaterielsEmailIfNotSentToday("admin@example.com");

            // Assert
            mockOperationNotification.Verify(n => n.NotifyTopUsedMateriels(It.IsAny<IEnumerable<ITStockM.Models.ViewModels.MaterielUsageViewModel>>(), It.Is<string>(s => s == "admin@example.com"), It.IsAny<string?>()), Times.Once);
        }
    }
}
