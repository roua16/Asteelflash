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
using Xunit;
using System.Linq;

namespace ITStockM.Tests.Services
{
    public class AssignmentServiceTests
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
        public async Task CreateAssignment_Should_Add_And_Save_And_Notify()
        {
            var ctx = CreateInMemoryContext("assignment_test_db");

            var mockAssignmentRepo = new Mock<IAssignmentRepository>();
            mockAssignmentRepo.Setup(r => r.Query()).Returns(Enumerable.Empty<ITStockM.Models.ITStockManagment.Assignment>().AsQueryable());
            mockAssignmentRepo.Setup(r => r.AddAsync(It.IsAny<Assignment>(), It.IsAny<System.Threading.CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            mockAssignmentRepo.Setup(r => r.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1).Verifiable();

            var mockRequestRepo = new Mock<IRequestRepository>();
            var mockDeliveryRepo = new Mock<IDeliveryOrderRepository>();
            var mockMaterielRepo = new Mock<IMaterielRepository>();

            var mockOperationNotification = new Mock<IOperationNotificationService>();
            mockOperationNotification.Setup(n => n.NotifyAssignmentCreated(It.IsAny<Assignment>(), It.IsAny<string?>())).Returns(Task.CompletedTask).Verifiable();

            var navigation = new TestNav();

            var assignmentServiceImpl = new ITStockM.Services.Assignments.AssignmentService(mockAssignmentRepo.Object, mockOperationNotification.Object);
            var service = assignmentServiceImpl;

            var newAssignment = new Assignment
            {
                AssignedBy = 1,
                Date = System.DateTime.UtcNow,
                Descipriton = "Test",
            };

            var created = await service.CreateAssignment(newAssignment);

            // allow background notification Task.Run to execute
            await Task.Delay(200);

            mockAssignmentRepo.Verify(r => r.AddAsync(It.IsAny<Assignment>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once);
            mockAssignmentRepo.Verify(r => r.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
            mockOperationNotification.Verify(n => n.NotifyAssignmentCreated(It.IsAny<Assignment>(), It.IsAny<string?>()), Times.AtLeastOnce);

            created.Should().NotBeNull();
        }
    }
}
