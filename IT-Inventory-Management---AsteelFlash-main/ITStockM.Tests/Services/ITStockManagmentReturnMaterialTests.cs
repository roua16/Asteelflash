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
    public class ITStockManagmentReturnMaterialTests
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
        public async Task RegisterReturnedMaterial_Creates_New_Material_And_Notifies()
        {
            // Arrange
            var ctx = CreateInMemoryContext("return_mat_db_1");

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
            mockOperationNotification.Setup(n => n.NotifyMaterielCreated(It.IsAny<Materiel>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask);

            var nav = new TestNav();

            var service = new ITStockManagmentService(ctx, null!, nav, mockExport.Object,
                mockAssignRepo.Object, mockReqRepo.Object, mockDeliveryRepo.Object, mockMaterielRepo.Object,
                mockAssignService.Object, mockReqService.Object, mockDeliveryService.Object, mockMaterielService.Object,
                mockOperationNotification.Object);

            // Act
            var created = await service.RegisterReturnedMaterial("Extra HDD", 2, "Good Conditions");

            // Assert
            created.Should().NotBeNull();
            created.MaterielName.Should().Be("Extra HDD");
            created.QuantityITStock.Should().Be(2);

            mockOperationNotification.Invocations
                .Select(i => i.Method.Name)
                .Should().Contain(nameof(IOperationNotificationService.NotifyMaterielCreated));
        }

        [Fact]
        public async Task RegisterReturnedMaterial_Updates_Existing_Material_And_Notifies()
        {
            // Arrange
            var ctx = CreateInMemoryContext("return_mat_db_2");

            var existing = new Materiel
            {
                MaterielName = "Existing Mouse",
                Type = "Hardware",
                QuantityITStock = 5,
                QuantityPDRStock = 0,
                IrreparableQuantity = 0,
                Repairing_Quantity = 0,
                Warranty = DateTime.UtcNow.AddYears(1)
            };

            await ctx.Materiels.AddAsync(existing);
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
            mockOperationNotification.Setup(n => n.NotifyMaterielUpdated(It.IsAny<Materiel>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask);

            var nav = new TestNav();

            var service = new ITStockManagmentService(ctx, null!, nav, mockExport.Object,
                mockAssignRepo.Object, mockReqRepo.Object, mockDeliveryRepo.Object, mockMaterielRepo.Object,
                mockAssignService.Object, mockReqService.Object, mockDeliveryService.Object, mockMaterielService.Object,
                mockOperationNotification.Object);

            // Act: return 3 more good ones
            var updated = await service.RegisterReturnedMaterial("Existing Mouse", 3, "Good Conditions");

            // Assert
            updated.Should().NotBeNull();
            updated.QuantityITStock.Should().Be(8);

            // allow background notification Task.Run to execute
            await Task.Delay(200);

            mockOperationNotification.Invocations
                .Select(i => i.Method.Name)
                .Should().Contain(nameof(IOperationNotificationService.NotifyMaterielUpdated));
        }
    }
}
