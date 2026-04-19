using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ITStockM.Data;
using ITStockM.Domain.Entities;
using ITStockM.Repositories;
using FluentAssertions;

namespace ITStockM.Tests.Repositories;

public class MaterielRepositoryTests
{
    private ITStockManagmentContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ITStockManagmentContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ITStockManagmentContext(options);
    }

    [Fact]
    public async Task AddAndGetByName_ShouldReturnInsertedMateriel()
    {
        var ctx = CreateInMemoryContext("test_db");
        var repo = new MaterielRepository(ctx);

        var materiel = new Materiel
        {
            MaterielName = "TestDevice",
            Type = "TypeA",
            QuantityITStock = 10,
            QuantityPDRStock = 0,
            IrreparableQuantity = 0,
            Repairing_Quantity = 0,
            Warranty = System.DateTime.UtcNow
        };

        await repo.AddAsync(materiel);
        await repo.SaveChangesAsync();

        var fetched = await repo.GetByNameAsync("TestDevice");
        fetched.Should().NotBeNull();
        fetched?.MaterielName.Should().Be("TestDevice");
    }
}
