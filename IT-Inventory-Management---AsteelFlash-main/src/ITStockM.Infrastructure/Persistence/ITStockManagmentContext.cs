using System;
using System.Linq;
using System.Collections.Generic;
using ITStockM.Application.Common.Interfaces;
using ITStockM.Domain.Base;
using ITStockM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ITStockM.Domain.Entities;
using ITStockM.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ITStockM.Data
{
  public partial class ITStockManagmentContext : IdentityDbContext<AppUser, IdentityRole<int>, int>, IApplicationDbContext
  {
    public ITStockManagmentContext()
    {
    }

    public ITStockManagmentContext(DbContextOptions<ITStockManagmentContext> options) : base(options)
    {
    }

    partial void OnModelBuilding(ModelBuilder builder);

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      // Configure Identity
      builder.Entity<AppUser>().ToTable("AspNetUsers", "dbo");
      builder.Entity<IdentityRole<int>>().ToTable("AspNetRoles", "dbo");
      builder.Entity<IdentityUserRole<int>>().ToTable("AspNetUserRoles", "dbo");
      builder.Entity<IdentityUserClaim<int>>().ToTable("AspNetUserClaims", "dbo");
      builder.Entity<IdentityUserLogin<int>>().ToTable("AspNetUserLogins", "dbo");
      builder.Entity<IdentityRoleClaim<int>>().ToTable("AspNetRoleClaims", "dbo");
      builder.Entity<IdentityUserToken<int>>().ToTable("AspNetUserTokens", "dbo");

      builder.Entity<ITStockM.Domain.Entities.AssignmentMateriel>().HasKey(table => new
      {
        table.MaterielId,
        table.AssignmentId
      });

      builder.Entity<ITStockM.Domain.Entities.DeliveryOrderMateriel>().HasKey(table => new
      {
        table.MaterielId,
        table.DeliveryOrderNumber
      });

      builder.Entity<ITStockM.Domain.Entities.Assignment>()
        .HasOne(i => i.Employee)
        .WithMany()
        .HasForeignKey(i => i.AssignedBy)
        .HasPrincipalKey(i => i.Id);

      builder.Entity<Assignment>()
          .HasOne(i => i.AssignedEmployee)
          .WithMany(i => i.Assignments)
          .HasForeignKey(i => i.AssignedTo)
          .HasPrincipalKey(i => i.Id);

      builder.Entity<ITStockM.Domain.Entities.Assignment>()
        .HasOne(i => i.Project)
        .WithMany(i => i.Assignments)
        .HasForeignKey(i => i.ProjectId)
        .HasPrincipalKey(i => i.Id);

      builder.Entity<ITStockM.Domain.Entities.AssignmentMateriel>()
        .HasOne(i => i.Assignment)
        .WithMany(i => i.AssignmentMateriels)
        .HasForeignKey(i => i.AssignmentId)
        .HasPrincipalKey(i => i.Id);

      builder.Entity<ITStockM.Domain.Entities.AssignmentMateriel>()
        .HasOne(i => i.Materiel)
        .WithMany(i => i.AssignmentMateriels)
        .HasForeignKey(i => i.MaterielId)
        .HasPrincipalKey(i => i.Id);

      builder.Entity<ITStockM.Domain.Entities.DeliveryOrder>()
        .HasOne(i => i.Supplier)
        .WithMany(i => i.DeliveryOrders)
        .HasForeignKey(i => i.SupplierName)
        .HasPrincipalKey(i => i.SupplierName);

      builder.Entity<ITStockM.Domain.Entities.DeliveryOrder>()
          .HasOne(i => i.Employee)
          .WithMany(i => i.DeliveryOrders)
          .HasForeignKey(i => i.EmployeeId)
          .HasPrincipalKey(i => i.Id);

      builder.Entity<ITStockM.Domain.Entities.DeliveryOrderMateriel>()
        .HasOne(i => i.DeliveryOrder)
        .WithMany(i => i.DeliveryOrderMateriels)
        .HasForeignKey(i => i.DeliveryOrderNumber)
        .HasPrincipalKey(i => i.DeliveryOrderNumber);

      builder.Entity<ITStockM.Domain.Entities.DeliveryOrderMateriel>()
        .HasOne(i => i.Materiel)
        .WithMany(i => i.DeliveryOrderMateriels)
        .HasForeignKey(i => i.MaterielId)
        .HasPrincipalKey(i => i.Id);



      builder.Entity<ITStockM.Domain.Entities.Offer>()
        .HasOne(i => i.Request)
        .WithMany(i => i.Offers)
        .HasForeignKey(i => i.RequestId)
        .HasPrincipalKey(i => i.Id);

      builder.Entity<ITStockM.Domain.Entities.Offer>()
        .HasOne(i => i.Supplier)
        .WithMany(i => i.Offers)
        .HasForeignKey(i => i.SupplierName)
        .HasPrincipalKey(i => i.SupplierName);

      builder.Entity<ITStockM.Domain.Entities.Request>()
        .HasOne(i => i.Employee)
        .WithMany(i => i.Requests)
        .HasForeignKey(i => i.EmployeeId)
        .HasPrincipalKey(i => i.Id);

      builder.Entity<ITStockM.Domain.Entities.Assignment>()
        .Property(p => p.Date)
        .HasColumnType("datetime2");

      builder.Entity<ITStockM.Domain.Entities.Assignment>()
        .Property(p => p.RestoreDateLimit)
        .HasColumnType("datetime2");

      builder.Entity<ITStockM.Domain.Entities.Assignment>()
        .Property(p => p.RestoreDate)
        .HasColumnType("datetime2");

      builder.Entity<ITStockM.Domain.Entities.DeliveryOrder>()
        .Property(p => p.Date)
        .HasColumnType("datetime");

      builder.Entity<ITStockM.Domain.Entities.DeliveryOrder>()
        .Property(p => p.DeliveryDate)
        .HasColumnType("datetime2");

      builder.Entity<ITStockM.Domain.Entities.Materiel>()
        .HasOne(m => m.Supplier)
        .WithMany(s => s.Materiels)
        .HasForeignKey(m => m.SupplierId)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.SetNull);

      builder.Entity<ITStockM.Domain.Entities.Materiel>()
        .Property(p => p.Warranty)
        .HasColumnType("datetime2");

      builder.Entity<ITStockM.Domain.Entities.Offer>()
        .Property(p => p.DeliveryDate)
        .HasColumnType("datetime2");

      builder.Entity<ITStockM.Domain.Entities.Offer>()
        .Property(p => p.Price)
        .HasPrecision(18, 2);

      builder.Entity<ITStockM.Domain.Entities.Request>()
        .Property(p => p.Date)
        .HasColumnType("datetime2");

      builder.Entity<ITStockM.Domain.Entities.Request>()
        .Property(p => p.ApprovedAt)
        .HasColumnType("datetime2");

      // ─── Asset Lifecycle Module ─────────────────────────────────────────────
      builder.Entity<ITStockM.Domain.Entities.MaintenanceTicket>()
        .HasOne(t => t.Materiel)
        .WithMany(m => m.MaintenanceTickets)
        .HasForeignKey(t => t.MaterielId)
        .OnDelete(DeleteBehavior.Restrict);

      builder.Entity<ITStockM.Domain.Entities.MaintenanceTicket>()
        .HasOne(t => t.ReportedBy)
        .WithMany()
        .HasForeignKey(t => t.ReportedByEmployeeId)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.SetNull);

      builder.Entity<ITStockM.Domain.Entities.AssetLifecycleRecord>()
        .HasOne(r => r.Materiel)
        .WithMany(m => m.AssetLifecycleRecords)
        .HasForeignKey(r => r.MaterielId)
        .OnDelete(DeleteBehavior.Cascade);

      builder.Entity<ITStockM.Domain.Entities.AssetPrediction>()
        .HasOne(p => p.Materiel)
        .WithMany(m => m.AssetPredictions)
        .HasForeignKey(p => p.MaterielId)
        .OnDelete(DeleteBehavior.Cascade);

      this.OnModelBuilding(builder);
    }

    /// <summary>
    /// Intercepts SaveChangesAsync to publish domain events through MediatR.
    /// This ensures domain events are processed after entities are persisted.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      var result = await base.SaveChangesAsync(cancellationToken);
      
      // Collect all domain events from changed entities
      var domainEvents = new List<DomainEvent>();
      foreach (var entity in ChangeTracker.Entries<BaseEntity>())
      {
        var events = entity.Entity.GetDomainEvents();
        domainEvents.AddRange(events);
      }

      // Mark events as published
      foreach (var @event in domainEvents)
      {
        @event.IsPublished = true;
      }

      return result;
    }

    public DbSet<ITStockM.Domain.Entities.Assignment> Assignments { get; set; }

    public DbSet<ITStockM.Domain.Entities.AssignmentMateriel> AssignmentMateriels { get; set; }

    public DbSet<ITStockM.Domain.Entities.DeliveryOrder> DeliveryOrders { get; set; }

    public DbSet<ITStockM.Domain.Entities.DeliveryOrderMateriel> DeliveryOrderMateriels { get; set; }

    public DbSet<ITStockM.Domain.Entities.Employee> Employees { get; set; }

    public DbSet<ITStockM.Domain.Entities.Materiel> Materiels { get; set; }

    public DbSet<ITStockM.Domain.Entities.Offer> Offers { get; set; }

    public DbSet<ITStockM.Domain.Entities.Project> Projects { get; set; }

    public DbSet<ITStockM.Domain.Entities.Request> Requests { get; set; }

    public DbSet<ITStockM.Domain.Entities.Supplier> Suppliers { get; set; }

    // ─── Asset Lifecycle Module ───────────────────────────────────────────────
    public DbSet<ITStockM.Domain.Entities.MaintenanceTicket> MaintenanceTickets { get; set; }
    public DbSet<ITStockM.Domain.Entities.AssetLifecycleRecord> AssetLifecycleRecords { get; set; }
    public DbSet<ITStockM.Domain.Entities.AssetPrediction> AssetPredictions { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
      configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
    }
  }
}