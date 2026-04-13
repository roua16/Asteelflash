using ITStockM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITStockM.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Assignment> Assignments { get; }
    DbSet<AssignmentMateriel> AssignmentMateriels { get; }
    DbSet<DeliveryOrder> DeliveryOrders { get; }
    DbSet<DeliveryOrderMateriel> DeliveryOrderMateriels { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Materiel> Materiels { get; }
    DbSet<Offer> Offers { get; }
    DbSet<Project> Projects { get; }
    DbSet<Request> Requests { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<MaintenanceTicket> MaintenanceTickets { get; }
    DbSet<AssetLifecycleRecord> AssetLifecycleRecords { get; }
    DbSet<AssetPrediction> AssetPredictions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
