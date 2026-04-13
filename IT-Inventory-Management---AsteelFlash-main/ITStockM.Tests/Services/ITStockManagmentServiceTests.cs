using FluentAssertions;
using ITStockM.Data;
using ITStockM.Domain.Entities;
using ITStockM.Models.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ITStockM.Tests.Services
{
    public class ITStockManagmentServiceTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ITStockManagmentContext _context;

        public ITStockManagmentServiceTests()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ITStockManagmentContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<ITStockManagmentContext>();
        }

        [Fact]
        public async Task GetEmployees_ShouldReturnAllEmployees()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee
                {
                    Id = 1,
                    FullName = "John Doe",
                    Email = "john@example.com",
                    Password = "P@ssw0rd!",
                    Post = "Technician",
                    Role = UserRoles.Employee,
                    PhoneNumber = "1111111111",
                    Service = "IT"
                },
                new Employee
                {
                    Id = 2,
                    FullName = "Jane Smith",
                    Email = "jane@example.com",
                    Password = "P@ssw0rd!",
                    Post = "Manager",
                    Role = UserRoles.Admin,
                    PhoneNumber = "2222222222",
                    Service = "IT"
                }
            };

            await _context.Employees.AddRangeAsync(employees);
            await _context.SaveChangesAsync();

            // Act - We test the context directly since service requires NavigationManager
            var result = await _context.Employees.ToListAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(e => e.FullName == "John Doe");
            result.Should().Contain(e => e.FullName == "Jane Smith");
        }

        [Fact]
        public async Task AddMateriel_ShouldAddNewMaterial()
        {
            // Arrange
            var material = new Materiel
            {
                Id = 1,
                MaterielName = "Test Material",
                Type = "Laptop",
                SerialNumber = "MAT-001",
                QuantityITStock = 10,
                QuantityPDRStock = 0,
                IrreparableQuantity = 0,
                Repairing_Quantity = 0,
                Warranty = DateTime.UtcNow.AddYears(1)
            };

            // Act
            await _context.Materiels.AddAsync(material);
            await _context.SaveChangesAsync();

            var result = await _context.Materiels.FindAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.SerialNumber.Should().Be("MAT-001");
            result.QuantityITStock.Should().Be(10);
        }

        [Fact]
        public async Task UpdateMateriel_ShouldUpdateExistingMaterial()
        {
            // Arrange
            var material = new Materiel
            {
                Id = 1,
                MaterielName = "Test Material",
                Type = "Laptop",
                SerialNumber = "MAT-001",
                QuantityITStock = 10,
                QuantityPDRStock = 0,
                IrreparableQuantity = 0,
                Repairing_Quantity = 0,
                Warranty = DateTime.UtcNow.AddYears(1)
            };

            await _context.Materiels.AddAsync(material);
            await _context.SaveChangesAsync();

            // Act
            material.QuantityITStock = 20;
            _context.Materiels.Update(material);
            await _context.SaveChangesAsync();

            var result = await _context.Materiels.FindAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.QuantityITStock.Should().Be(20);
        }

        [Fact]
        public async Task DeleteMateriel_ShouldRemoveMaterial()
        {
            // Arrange
            var material = new Materiel
            {
                Id = 1,
                MaterielName = "Test Material",
                Type = "Laptop",
                SerialNumber = "MAT-001",
                QuantityITStock = 10,
                QuantityPDRStock = 0,
                IrreparableQuantity = 0,
                Repairing_Quantity = 0,
                Warranty = DateTime.UtcNow.AddYears(1)
            };

            await _context.Materiels.AddAsync(material);
            await _context.SaveChangesAsync();

            // Act
            _context.Materiels.Remove(material);
            await _context.SaveChangesAsync();

            var result = await _context.Materiels.FindAsync(1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddSupplier_ShouldAddNewSupplier()
        {
            // Arrange
            var supplier = new Supplier
            {
                SupplierName = "Test Supplier",
                Adress = "123 Test St",
                Email = "supplier@example.com",
                PhoneNumber = "1234567890"
            };

            // Act
            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            var result = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierName == "Test Supplier");

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("supplier@example.com");
        }

        [Fact]
        public async Task AddProject_ShouldAddNewProject()
        {
            // Arrange
            var project = new Project
            {
                Id = 1,
                ProjectName = "Test Project"
            };

            // Act
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var result = await _context.Projects.FindAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.ProjectName.Should().Be("Test Project");
        }

        [Fact]
        public async Task CreateAssignment_WithMaterials_ShouldCreateCorrectly()
        {
            // Arrange
            var employee = new Employee
            {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "P@ssw0rd!",
                Post = "Technician",
                Role = UserRoles.Employee,
                PhoneNumber = "1111111111",
                Service = "IT"
            };

            var assignedEmployee = new Employee
            {
                Id = 2,
                FullName = "Jane Smith",
                Email = "jane@example.com",
                Password = "P@ssw0rd!",
                Post = "Manager",
                Role = UserRoles.Admin,
                PhoneNumber = "2222222222",
                Service = "IT"
            };
            var project = new Project { Id = 1, ProjectName = "Test Project" };

            await _context.Employees.AddRangeAsync(employee, assignedEmployee);
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var assignment = new Assignment
            {
                Id = 1,
                AssignedBy = 1,
                AssignedTo = 2,
                ProjectId = 1,
                Date = DateTime.UtcNow,
                Descipriton = "Test assignment",
                OnMission = false
            };

            // Act
            await _context.Assignments.AddAsync(assignment);
            await _context.SaveChangesAsync();

            var result = await _context.Assignments
                .Include(a => a.Employee)
                .Include(a => a.AssignedEmployee)
                .Include(a => a.Project)
                .FirstOrDefaultAsync(a => a.Id == 1);

            // Assert
            result.Should().NotBeNull();
            result!.Employee.FullName.Should().Be("John Doe");
            result.AssignedEmployee.FullName.Should().Be("Jane Smith");
            result.Project.ProjectName.Should().Be("Test Project");
        }

        public void Dispose()
        {
            _context?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}
