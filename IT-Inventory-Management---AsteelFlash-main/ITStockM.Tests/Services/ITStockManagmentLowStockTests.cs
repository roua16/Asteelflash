using System.Threading.Tasks;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using ITStockM.Data;
using ITStockM.Models.ITStockManagment;
using ITStockM.Repositories;
using ITStockM.Services;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Assignments;
using ITStockM.Services.Requests;
using ITStockM.Services.DeliveryOrders;
using ITStockM.Services.Materiels;
using Services = global::ITStockM.Services;
using Xunit;
using System;

namespace ITStockM.Tests.Services
{
    public class ITStockManagmentLowStockTests
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
        public async Task LowStockNotification_IsSent_When_Crossing_Threshold()
        {
            // Arrange
            var ctx = CreateInMemoryContext("lowstock_db_1");

            var materiel = new Materiel
            {
                Id = 1,
                MaterielName = "Test Material",
                Type = "Consumable",
                QuantityITStock = 12,
                QuantityPDRStock = 0,
                IrreparableQuantity = 0,
                Repairing_Quantity = 0,
                Warranty = DateTime.UtcNow.AddYears(1)
            };

            await ctx.Materiels.AddAsync(materiel);
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
            mockOperationNotification.Setup(n => n.NotifyMaterielLowStock(It.IsAny<Materiel>(), It.IsAny<int>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask).Verifiable();

            var nav = new TestNav();

            var service = new ITStockManagmentService(ctx, null!, nav, mockExport.Object,
                mockAssignRepo.Object, mockReqRepo.Object, mockDeliveryRepo.Object, mockMaterielRepo.Object,
                mockAssignService.Object, mockReqService.Object, mockDeliveryService.Object, mockMaterielService.Object,
                mockOperationNotification.Object);

            // Act: decrease quantity to cross threshold (default threshold=10)
            var updated = new Materiel {
                Id = 1,
                MaterielName = materiel.MaterielName,
                Type = materiel.Type,
                QuantityITStock = 8,
                QuantityPDRStock = 0,
                IrreparableQuantity = 0,
                Repairing_Quantity = 0,
                Warranty = materiel.Warranty
            };

            await service.UpdateMateriel(1, updated);

            // allow async notification Task.Run to execute
            await Task.Delay(200);

            mockOperationNotification.Verify(n => n.NotifyMaterielLowStock(It.IsAny<Materiel>(), It.Is<int>(t => t == 10), It.IsAny<string?>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task LowStockNotification_NotSent_When_Not_Crossing_Threshold()
        {
            // Arrange
            var ctx = CreateInMemoryContext("lowstock_db_2");

            var materiel = new Materiel
            {
                Id = 2,
                MaterielName = "Test Material 2",
                Type = "Consumable",
                QuantityITStock = 9,
                QuantityPDRStock = 0,
                IrreparableQuantity = 0,
                Repairing_Quantity = 0,
                Warranty = DateTime.UtcNow.AddYears(1)
            };

            await ctx.Materiels.AddAsync(materiel);
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
            mockOperationNotification.Setup(n => n.NotifyMaterielLowStock(It.IsAny<Materiel>(), It.IsAny<int>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask).Verifiable();

            var nav = new TestNav();

            var service = new ITStockManagmentService(ctx, null!, nav, mockExport.Object,
                mockAssignRepo.Object, mockReqRepo.Object, mockDeliveryRepo.Object, mockMaterielRepo.Object,
                mockAssignService.Object, mockReqService.Object, mockDeliveryService.Object, mockMaterielService.Object,
                mockOperationNotification.Object);

            // Act: update without crossing (e.g., still below threshold)
            var updated = new Materiel {
                Id = 2,
                MaterielName = materiel.MaterielName,
                Type = materiel.Type,
                QuantityITStock = 8, // already below threshold
                QuantityPDRStock = 0,
                IrreparableQuantity = 0,
                Repairing_Quantity = 0,
                Warranty = materiel.Warranty
            };

            await service.UpdateMateriel(2, updated);

            await Task.Delay(200);

            mockOperationNotification.Verify(n => n.NotifyMaterielLowStock(It.IsAny<Materiel>(), It.IsAny<int>(), It.IsAny<string?>()), Times.Never);
        }
    }
}
