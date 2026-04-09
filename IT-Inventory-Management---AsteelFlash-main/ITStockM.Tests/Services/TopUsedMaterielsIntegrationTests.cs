using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

using ITStockM.Data;
using ITStockM.Models.ITStockManagment;
using ITStockM.Models.Constants;
using ITStockM.Models.ViewModels;
using ITStockM.Repositories;
using ITStockM.Services;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Implementation;

namespace ITStockM.Tests.Services
{
    /// <summary>
    /// Integration tests simulating the full dashboard open → email flow
    /// for Admin/PDR users receiving top-3 materials usage notification.
    /// </summary>
    public class TopUsedMaterielsIntegrationTests
    {
        private ITStockManagmentContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ITStockManagmentContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ITStockManagmentContext(options);
        }

        private class TestNav : Microsoft.AspNetCore.Components.NavigationManager
        {
            public TestNav()
            {
                Initialize("http://localhost/", "http://localhost/");
            }
            protected override void NavigateToCore(string uri, bool forceLoad) { }
        }

        /// <summary>
        /// Simulates Admin opening dashboard and receiving top-3 email.
        /// Tests the complete flow: GetTopUsedMateriels + SendTopUsedMaterielsEmailIfNotSentToday
        /// </summary>
        [Fact]
        public async Task DashboardOpen_Admin_ReceivesTop3Email()
        {
            // Arrange - Set up database with usage data
            var ctx = CreateInMemoryContext("integration_admin_dashboard");

            // Create materials
            var m1 = new Materiel { Id = 1, MaterielName = "Laptop", Type = "Consumable", QuantityITStock = 10, QuantityPDRStock = 5, Warranty = DateTime.UtcNow };
            var m2 = new Materiel { Id = 2, MaterielName = "Mouse", Type = "Consumable", QuantityITStock = 20, QuantityPDRStock = 10, Warranty = DateTime.UtcNow };
            var m3 = new Materiel { Id = 3, MaterielName = "Keyboard", Type = "Consumable", QuantityITStock = 15, QuantityPDRStock = 8, Warranty = DateTime.UtcNow };
            var m4 = new Materiel { Id = 4, MaterielName = "Monitor", Type = "Consumable", QuantityITStock = 8, QuantityPDRStock = 4, Warranty = DateTime.UtcNow };
            await ctx.Materiels.AddRangeAsync(m1, m2, m3, m4);

            // Create assignments with usage
            await ctx.AssignmentMateriels.AddRangeAsync(
                new AssignmentMateriel { MaterielId = 1, Qte = 10, AssignmentId = 1 },
                new AssignmentMateriel { MaterielId = 2, Qte = 25, AssignmentId = 1 },
                new AssignmentMateriel { MaterielId = 3, Qte = 15, AssignmentId = 2 },
                new AssignmentMateriel { MaterielId = 4, Qte = 5, AssignmentId = 2 }
            );

            // Create delivery orders with usage
            await ctx.DeliveryOrderMateriels.AddRangeAsync(
                new DeliveryOrderMateriel { MaterielId = 1, Qte = 5, DeliveryOrderNumber = "DO-1" },
                new DeliveryOrderMateriel { MaterielId = 2, Qte = 10, DeliveryOrderNumber = "DO-1" }
            );

            await ctx.SaveChangesAsync();

            // Set up mocks
            var mockExport = new Mock<ITStockM.Services.Export.IExportService>();
            var mockAssignRepo = new Mock<IAssignmentRepository>();
            var mockReqRepo = new Mock<IRequestRepository>();
            var mockDeliveryRepo = new Mock<IDeliveryOrderRepository>();
            var mockMaterielRepo = new Mock<IMaterielRepository>();
            var mockAssignService = new Mock<global::ITStockM.Services.Assignments.IAssignmentService>();
            var mockReqService = new Mock<global::ITStockM.Services.Requests.IRequestService>();
            var mockDeliveryService = new Mock<global::ITStockM.Services.DeliveryOrders.IDeliveryOrderService>();
            var mockMaterielService = new Mock<global::ITStockM.Services.Materiels.IMaterielService>();

            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var operationNotificationService = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var nav = new TestNav();

            var service = new ITStockManagmentService(ctx, null!, nav, mockExport.Object,
                mockAssignRepo.Object, mockReqRepo.Object, mockDeliveryRepo.Object, mockMaterielRepo.Object,
                mockAssignService.Object, mockReqService.Object, mockDeliveryService.Object, mockMaterielService.Object,
                operationNotificationService);

            // Act - Simulate Admin opening dashboard
            var adminEmail = "admin@asteelflash.com";
            
            // This is what happens in Index.razor.cs OnInitializedAsync
            var top3 = await service.GetTopUsedMateriels(3);
            
            // Check role and send email (simulating Admin role)
            var userRole = UserRoles.Admin;
            if (string.Equals(userRole, UserRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(userRole, UserRoles.PDR, StringComparison.OrdinalIgnoreCase))
            {
                await service.SendTopUsedMaterielsEmailIfNotSentToday(adminEmail);
            }

            // Assert - Email should be sent
            mockEmail.Verify(e => e.SendEmailAsync(
                It.Is<string>(s => s == adminEmail),
                It.Is<string>(s => s.Contains("Top 3 Most Used Materials")),
                It.IsAny<string>()), Times.Once);

            // Verify top 3 calculation
            top3.Should().HaveCount(3);
            top3[0].MaterielName.Should().Be("Mouse"); // 25 + 10 = 35
            top3[0].UsageCount.Should().Be(35);

            // The next two entries are tied at 15, so ordering is not guaranteed.
            top3.Skip(1).Select(t => t.MaterielName)
                .Should().BeEquivalentTo(new[] { "Keyboard", "Laptop" });
            top3.Skip(1).All(t => t.UsageCount == 15).Should().BeTrue();
            
            // Verify usage share: the service computes share against total usage across all used materials.
            top3[0].UsageShare.Should().BeApproximately(0.5, 0.001);
        }

        /// <summary>
        /// Simulates PDR opening dashboard and receiving top-3 email.
        /// Verifies both Admin and PDR can receive the email.
        /// </summary>
        [Fact]
        public async Task DashboardOpen_PDR_ReceivesTop3Email()
        {
            // Arrange
            var ctx = CreateInMemoryContext("integration_pdr_dashboard");

            var m1 = new Materiel { Id = 1, MaterielName = "Laptop", Type = "Consumable", QuantityITStock = 10, QuantityPDRStock = 5, Warranty = DateTime.UtcNow };
            var m2 = new Materiel { Id = 2, MaterielName = "Mouse", Type = "Consumable", QuantityITStock = 20, QuantityPDRStock = 10, Warranty = DateTime.UtcNow };
            await ctx.Materiels.AddRangeAsync(m1, m2);

            await ctx.AssignmentMateriels.AddRangeAsync(
                new AssignmentMateriel { MaterielId = 1, Qte = 8, AssignmentId = 1 },
                new AssignmentMateriel { MaterielId = 2, Qte = 12, AssignmentId = 1 }
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

            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var operationNotificationService = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var nav = new TestNav();

            var service = new ITStockManagmentService(ctx, null!, nav, mockExport.Object,
                mockAssignRepo.Object, mockReqRepo.Object, mockDeliveryRepo.Object, mockMaterielRepo.Object,
                mockAssignService.Object, mockReqService.Object, mockDeliveryService.Object, mockMaterielService.Object,
                operationNotificationService);

            // Act - Simulate PDR opening dashboard
            var pdrEmail = "pdr@asteelflash.com";
            var userRole = UserRoles.PDR;

            var top3 = await service.GetTopUsedMateriels(3);
            
            if (string.Equals(userRole, UserRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(userRole, UserRoles.PDR, StringComparison.OrdinalIgnoreCase))
            {
                await service.SendTopUsedMaterielsEmailIfNotSentToday(pdrEmail);
            }

            // Assert
            mockEmail.Verify(e => e.SendEmailAsync(
                It.Is<string>(s => s == pdrEmail),
                It.Is<string>(s => s.Contains("Top 3 Most Used Materials")),
                It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Verifies that when non-Admin/PDR user (e.g., Employee) opens dashboard,
        /// no email is sent.
        /// </summary>
        [Fact]
        public async Task DashboardOpen_Employee_DoesNotReceiveTop3Email()
        {
            // Arrange
            var ctx = CreateInMemoryContext("integration_employee_dashboard");

            var m1 = new Materiel { Id = 1, MaterielName = "Laptop", Type = "Consumable", QuantityITStock = 10, QuantityPDRStock = 5, Warranty = DateTime.UtcNow };
            await ctx.Materiels.AddAsync(m1);

            await ctx.AssignmentMateriels.AddAsync(
                new AssignmentMateriel { MaterielId = 1, Qte = 5, AssignmentId = 1 }
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

            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var operationNotificationService = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var nav = new TestNav();

            var service = new ITStockManagmentService(ctx, null!, nav, mockExport.Object,
                mockAssignRepo.Object, mockReqRepo.Object, mockDeliveryRepo.Object, mockMaterielRepo.Object,
                mockAssignService.Object, mockReqService.Object, mockDeliveryService.Object, mockMaterielService.Object,
                operationNotificationService);

            // Act - Simulate Employee opening dashboard
            var employeeEmail = "employee@asteelflash.com";
            var userRole = "Employee"; // Not Admin or PDR

            var top3 = await service.GetTopUsedMateriels(3);
            
            if (string.Equals(userRole, UserRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(userRole, UserRoles.PDR, StringComparison.OrdinalIgnoreCase))
            {
                await service.SendTopUsedMaterielsEmailIfNotSentToday(employeeEmail);
            }

            // Assert - No email should be sent for Employee role
            mockEmail.Verify(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Verifies that second dashboard open on same day does NOT resend email (once-per-day rule).
        /// </summary>
        [Fact]
        public async Task DashboardOpen_SameDay_SecondTime_DoesNotResendEmail()
        {
            // Arrange
            var ctx = CreateInMemoryContext("integration_same_day");

            var m1 = new Materiel { Id = 1, MaterielName = "Laptop", Type = "Consumable", QuantityITStock = 10, QuantityPDRStock = 5, Warranty = DateTime.UtcNow };
            await ctx.Materiels.AddAsync(m1);

            await ctx.AssignmentMateriels.AddAsync(
                new AssignmentMateriel { MaterielId = 1, Qte = 5, AssignmentId = 1 }
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

            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var operationNotificationService = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var nav = new TestNav();

            var service = new ITStockManagmentService(ctx, null!, nav, mockExport.Object,
                mockAssignRepo.Object, mockReqRepo.Object, mockDeliveryRepo.Object, mockMaterielRepo.Object,
                mockAssignService.Object, mockReqService.Object, mockDeliveryService.Object, mockMaterielService.Object,
                operationNotificationService);

            // Act - First dashboard open
            var adminEmail = "admin@asteelflash.com";
            var userRole = UserRoles.Admin;

            var top3 = await service.GetTopUsedMateriels(3);
            
            if (string.Equals(userRole, UserRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(userRole, UserRoles.PDR, StringComparison.OrdinalIgnoreCase))
            {
                await service.SendTopUsedMaterielsEmailIfNotSentToday(adminEmail);
            }

            // Act - Second dashboard open (same day, e.g., user refreshed page)
            await service.SendTopUsedMaterielsEmailIfNotSentToday(adminEmail);

            // Assert - Email should be sent only once
            mockEmail.Verify(e => e.SendEmailAsync(
                It.Is<string>(s => s == adminEmail),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Verifies email content includes correct material names, usage counts, and shares.
        /// </summary>
        [Fact]
        public async Task DashboardOpen_EmailContent_IsCorrect()
        {
            // Arrange
            var ctx = CreateInMemoryContext("integration_email_content");

            var m1 = new Materiel { Id = 1, MaterielName = "Laptop", Type = "Consumable", QuantityITStock = 10, QuantityPDRStock = 5, Warranty = DateTime.UtcNow };
            var m2 = new Materiel { Id = 2, MaterielName = "Mouse", Type = "Consumable", QuantityITStock = 20, QuantityPDRStock = 10, Warranty = DateTime.UtcNow };
            var m3 = new Materiel { Id = 3, MaterielName = "Keyboard", Type = "Consumable", QuantityITStock = 15, QuantityPDRStock = 8, Warranty = DateTime.UtcNow };
            await ctx.Materiels.AddRangeAsync(m1, m2, m3);

            // Usage: Laptop=10, Mouse=20, Keyboard=5
            await ctx.AssignmentMateriels.AddRangeAsync(
                new AssignmentMateriel { MaterielId = 1, Qte = 10, AssignmentId = 1 },
                new AssignmentMateriel { MaterielId = 2, Qte = 20, AssignmentId = 1 },
                new AssignmentMateriel { MaterielId = 3, Qte = 5, AssignmentId = 1 }
            );

            await ctx.SaveChangesAsync();

            string? capturedSubject = null;
            string? capturedBody = null;

            var mockExport = new Mock<ITStockM.Services.Export.IExportService>();
            var mockAssignRepo = new Mock<IAssignmentRepository>();
            var mockReqRepo = new Mock<IRequestRepository>();
            var mockDeliveryRepo = new Mock<IDeliveryOrderRepository>();
            var mockMaterielRepo = new Mock<IMaterielRepository>();
            var mockAssignService = new Mock<global::ITStockM.Services.Assignments.IAssignmentService>();
            var mockReqService = new Mock<global::ITStockM.Services.Requests.IRequestService>();
            var mockDeliveryService = new Mock<global::ITStockM.Services.DeliveryOrders.IDeliveryOrderService>();
            var mockMaterielService = new Mock<global::ITStockM.Services.Materiels.IMaterielService>();

            var mockEmail = new Mock<IEmailService>();
            mockEmail.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((to, subject, body) =>
                {
                    capturedSubject = subject;
                    capturedBody = body;
                })
                .Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var operationNotificationService = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var nav = new TestNav();

            var service = new ITStockManagmentService(ctx, null!, nav, mockExport.Object,
                mockAssignRepo.Object, mockReqRepo.Object, mockDeliveryRepo.Object, mockMaterielRepo.Object,
                mockAssignService.Object, mockReqService.Object, mockDeliveryService.Object, mockMaterielService.Object,
                operationNotificationService);

            // Act
            var adminEmail = "admin@asteelflash.com";
            var top3 = await service.GetTopUsedMateriels(3);
            await service.SendTopUsedMaterielsEmailIfNotSentToday(adminEmail);

            // Assert - Verify email content
            capturedSubject.Should().Contain("Top 3 Most Used Materials");
            
            capturedBody.Should().Contain("Mouse");
            capturedBody.Should().Contain("20");
            
            capturedBody.Should().Contain("Laptop");
            capturedBody.Should().Contain("10");
            
            capturedBody.Should().Contain("Keyboard");
            capturedBody.Should().Contain("5");

            // Verify percentages (Mouse: 20/35 = 57.1%, Laptop: 10/35 = 28.6%, Keyboard: 5/35 = 14.3%)
            capturedBody.Should().MatchRegex("57[\\.,]1%");
            capturedBody.Should().MatchRegex("28[\\.,]6%");
            capturedBody.Should().MatchRegex("14[\\.,]3%");
        }
    }
}

