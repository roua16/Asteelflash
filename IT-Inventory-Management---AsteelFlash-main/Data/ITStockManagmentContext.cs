using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ITStockM.Models.ITStockManagment;

namespace ITStockM.Data
{
    public partial class ITStockManagmentContext : DbContext
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

            builder.Entity<ITStockM.Models.ITStockManagment.AssignmentMateriel>().HasKey(table => new {
                table.MaterielId, table.AssignmentId
            });

            builder.Entity<ITStockM.Models.ITStockManagment.DeliveryOrderMateriel>().HasKey(table => new {
                table.MaterielId, table.DeliveryOrderNumber
            });

            builder.Entity<ITStockM.Models.ITStockManagment.Assignment>()
              .HasOne(i => i.Employee)
              .WithMany()
              .HasForeignKey(i => i.AssignedBy)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Assignment>()
                .HasOne(i => i.AssignedEmployee)
                .WithMany(i => i.Assignments)
                .HasForeignKey(i => i.AssignedTo)
                .HasPrincipalKey(i => i.Id);

            builder.Entity<ITStockM.Models.ITStockManagment.Assignment>()
              .HasOne(i => i.Project)
              .WithMany(i => i.Assignments)
              .HasForeignKey(i => i.ProjectId)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<ITStockM.Models.ITStockManagment.AssignmentMateriel>()
              .HasOne(i => i.Assignment)
              .WithMany(i => i.AssignmentMateriels)
              .HasForeignKey(i => i.AssignmentId)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<ITStockM.Models.ITStockManagment.AssignmentMateriel>()
              .HasOne(i => i.Materiel)
              .WithMany(i => i.AssignmentMateriels)
              .HasForeignKey(i => i.MaterielId)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<ITStockM.Models.ITStockManagment.DeliveryOrder>()
              .HasOne(i => i.Supplier)
              .WithMany(i => i.DeliveryOrders)
              .HasForeignKey(i => i.SupplierName)
              .HasPrincipalKey(i => i.SupplierName);

            builder.Entity<ITStockM.Models.ITStockManagment.DeliveryOrder>()
                .HasOne(i => i.Employee)
                .WithMany(i => i.DeliveryOrders)
                .HasForeignKey(i => i.EmployeeId)
                .HasPrincipalKey(i => i.Id);

            builder.Entity<ITStockM.Models.ITStockManagment.DeliveryOrderMateriel>()
              .HasOne(i => i.DeliveryOrder)
              .WithMany(i => i.DeliveryOrderMateriels)
              .HasForeignKey(i => i.DeliveryOrderNumber)
              .HasPrincipalKey(i => i.DeleveryOrderNumber);

            builder.Entity<ITStockM.Models.ITStockManagment.DeliveryOrderMateriel>()
              .HasOne(i => i.Materiel)
              .WithMany(i => i.DeliveryOrderMateriels)
              .HasForeignKey(i => i.MaterielId)
              .HasPrincipalKey(i => i.Id);

         

            builder.Entity<ITStockM.Models.ITStockManagment.Offer>()
              .HasOne(i => i.Request)
              .WithMany(i => i.Offers)
              .HasForeignKey(i => i.RequestId)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<ITStockM.Models.ITStockManagment.Offer>()
              .HasOne(i => i.Supplier)
              .WithMany(i => i.Offers)
              .HasForeignKey(i => i.SupplierName)
              .HasPrincipalKey(i => i.SupplierName);

            builder.Entity<ITStockM.Models.ITStockManagment.Request>()
              .HasOne(i => i.Employee)
              .WithMany(i => i.Requests)
              .HasForeignKey(i => i.EmployeeId)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<ITStockM.Models.ITStockManagment.Assignment>()
              .Property(p => p.Date)
              .HasColumnType("datetime2");

            builder.Entity<ITStockM.Models.ITStockManagment.Assignment>()
              .Property(p => p.RestoreDateLimit)
              .HasColumnType("datetime2");

            builder.Entity<ITStockM.Models.ITStockManagment.Assignment>()
              .Property(p => p.RestoreDate)
              .HasColumnType("datetime2");

            builder.Entity<ITStockM.Models.ITStockManagment.DeliveryOrder>()
              .Property(p => p.Date)
              .HasColumnType("datetime");

            builder.Entity<ITStockM.Models.ITStockManagment.DeliveryOrder>()
              .Property(p => p.DeliveryDate)
              .HasColumnType("datetime2");

            builder.Entity<ITStockM.Models.ITStockManagment.Materiel>()
              .Property(p => p.Warranty)
              .HasColumnType("datetime2");

            builder.Entity<ITStockM.Models.ITStockManagment.Offer>()
              .Property(p => p.DeliveryDate)
              .HasColumnType("datetime2");

            builder.Entity<ITStockM.Models.ITStockManagment.Request>()
              .Property(p => p.Date)
              .HasColumnType("datetime2");

            builder.Entity<ITStockM.Models.ITStockManagment.Request>()
              .Property(p => p.ApprovedAt)
              .HasColumnType("datetime2");
            this.OnModelBuilding(builder);
        }

        public DbSet<ITStockM.Models.ITStockManagment.Assignment> Assignments { get; set; }

        public DbSet<ITStockM.Models.ITStockManagment.AssignmentMateriel> AssignmentMateriels { get; set; }

        public DbSet<ITStockM.Models.ITStockManagment.DeliveryOrder> DeliveryOrders { get; set; }

        public DbSet<ITStockM.Models.ITStockManagment.DeliveryOrderMateriel> DeliveryOrderMateriels { get; set; }

        public DbSet<ITStockM.Models.ITStockManagment.Employee> Employees { get; set; }

        public DbSet<ITStockM.Models.ITStockManagment.Materiel> Materiels { get; set; }

        public DbSet<ITStockM.Models.ITStockManagment.Offer> Offers { get; set; }

        public DbSet<ITStockM.Models.ITStockManagment.Project> Projects { get; set; }

        public DbSet<ITStockM.Models.ITStockManagment.Request> Requests { get; set; }

        public DbSet<ITStockM.Models.ITStockManagment.Supplier> Suppliers { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
        }
    }
}