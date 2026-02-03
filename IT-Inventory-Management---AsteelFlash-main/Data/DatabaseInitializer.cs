using ITStockM.Models.ITStockManagment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ITStockM.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(
            ITStockManagmentContext context,
            IHostEnvironment environment,
            IConfiguration configuration,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Database migrations applied.");

            var seedSection = configuration.GetSection("Seed");
            var seedEnabled = seedSection.GetValue("Enabled", environment.IsDevelopment());
            var seedDemoData = seedSection.GetValue("DemoData", environment.IsDevelopment());

            // Check if this is the first run by checking if any employees exist
            var isFirstRun = !await context.Employees.AnyAsync(cancellationToken);

            // Clearing is destructive; only do it on first run in development and if explicitly enabled
            var clearOldData = seedSection.GetValue("ClearOldData", false);
            if (clearOldData && environment.IsDevelopment() && isFirstRun)
            {
                await ClearOldDataAsync(context, logger, cancellationToken);
            }

            if (!seedEnabled)
            {
                logger.LogInformation("Seeding disabled (Seed:Enabled=false).");
                return;
            }

            // Only seed if this is the first run (no employees exist)
            if (!isFirstRun)
            {
                logger.LogInformation("Database already seeded. Skipping seed operations.");
                return;
            }

            logger.LogInformation("First run detected. Starting database seeding...");

            await SeedAdminAsync(context, seedSection, logger, cancellationToken);

            if (seedDemoData)
            {
                await SeedDemoDataAsync(context, logger, cancellationToken);
            }
            else
            {
                logger.LogInformation("Demo data seeding disabled (Seed:DemoData=false).");
            }

            logger.LogInformation("Database seeding complete.");
        }

        private static async Task SeedAdminAsync(
            ITStockManagmentContext context,
            IConfiguration seedSection,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var adminEmail = seedSection["AdminEmail"] ?? "admin@asteelflash.com";
            var adminPassword = seedSection["AdminPassword"] ?? "admin123";
            var adminFullName = seedSection["AdminFullName"] ?? "Admin User";
            var adminPhoneNumber = seedSection["AdminPhoneNumber"] ?? "+0000000000";
            var adminService = seedSection["AdminService"] ?? "IT";

            var admin = await context.Employees.FirstOrDefaultAsync(e => e.Email == adminEmail, cancellationToken);
            if (admin == null)
            {
                admin = new Employee
                {
                    FullName = adminFullName,
                    Email = adminEmail,
                    Password = adminPassword,
                    Post = "Admin",
                    PhoneNumber = adminPhoneNumber,
                    Service = adminService
                };

                context.Employees.Add(admin);
                await context.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Seeded admin user: {AdminEmail}", adminEmail);
            }
            else
            {
                if (!string.Equals(admin.Post, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    admin.Post = "Admin";
                    await context.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Updated existing user to Admin: {AdminEmail}", adminEmail);
                }
            }
        }

        private static async Task SeedDemoDataAsync(
            ITStockManagmentContext context,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            // Employees (non-admin)
            if (!await context.Employees.AnyAsync(e => e.Email == "john.smith@asteelflash.com", cancellationToken))
            {
                var employees = new[]
                {
                    new Employee
                    {
                        FullName = "John Smith",
                        Email = "john.smith@asteelflash.com",
                        Password = "password123",
                        Post = "Software Developer",
                        PhoneNumber = "+1234567891",
                        Service = "Development"
                    },
                    new Employee
                    {
                        FullName = "Sarah Johnson",
                        Email = "sarah.johnson@asteelflash.com",
                        Password = "password123",
                        Post = "Project Manager",
                        PhoneNumber = "+1234567892",
                        Service = "Management"
                    },
                    new Employee
                    {
                        FullName = "Mike Davis",
                        Email = "mike.davis@asteelflash.com",
                        Password = "password123",
                        Post = "IT Support",
                        PhoneNumber = "+1234567893",
                        Service = "IT"
                    }
                };
                context.Employees.AddRange(employees);
                await context.SaveChangesAsync(cancellationToken);
            }

            // Suppliers
            if (!await context.Suppliers.AnyAsync(cancellationToken))
            {
                var suppliers = new[]
                {
                    new Supplier
                    {
                        SupplierName = "TechCorp Supplies",
                        Adress = "123 Tech Street, Silicon Valley, CA",
                        Email = "orders@techcorp.com",
                        PhoneNumber = "+1-555-0101"
                    },
                    new Supplier
                    {
                        SupplierName = "Global IT Solutions",
                        Adress = "456 Business Ave, New York, NY",
                        Email = "sales@globalit.com",
                        PhoneNumber = "+1-555-0102"
                    },
                    new Supplier
                    {
                        SupplierName = "Office Equipment Plus",
                        Adress = "789 Commerce Blvd, Chicago, IL",
                        Email = "info@officeplus.com",
                        PhoneNumber = "+1-555-0103"
                    }
                };
                context.Suppliers.AddRange(suppliers);
                await context.SaveChangesAsync(cancellationToken);
            }

            // Projects
            if (!await context.Projects.AnyAsync(cancellationToken))
            {
                var projects = new[]
                {
                    new Project { ProjectName = "Website Redesign" },
                    new Project { ProjectName = "Mobile App Development" },
                    new Project { ProjectName = "Database Migration" },
                    new Project { ProjectName = "Network Infrastructure Upgrade" }
                };
                context.Projects.AddRange(projects);
                await context.SaveChangesAsync(cancellationToken);
            }

            // Materials
            if (!await context.Materiels.AnyAsync(cancellationToken))
            {
                var materials = new[]
                {
                    new Materiel
                    {
                        MaterielName = "Dell Latitude 5420 Laptop",
                        Type = "Laptop",
                        SerialNumber = "DL5420-001",
                        QuantityITStock = 5,
                        QuantityPDRStock = 0,
                        IrreparableQuantity = 0,
                        Repairing_Quantity = 0,
                        Warranty = DateTime.Now.AddYears(2)
                    },
                    new Materiel
                    {
                        MaterielName = "HP LaserJet Pro M182nw",
                        Type = "Printer",
                        SerialNumber = "HP182-001",
                        QuantityITStock = 2,
                        QuantityPDRStock = 0,
                        IrreparableQuantity = 0,
                        Repairing_Quantity = 0,
                        Warranty = DateTime.Now.AddYears(1)
                    },
                    new Materiel
                    {
                        MaterielName = "Cisco Router 2901",
                        Type = "Network Equipment",
                        SerialNumber = "C2901-001",
                        QuantityITStock = 1,
                        QuantityPDRStock = 0,
                        IrreparableQuantity = 0,
                        Repairing_Quantity = 0,
                        Warranty = DateTime.Now.AddMonths(6)
                    },
                    new Materiel
                    {
                        MaterielName = "Microsoft Office 365 License",
                        Type = "Software License",
                        SerialNumber = "MS365-001",
                        QuantityITStock = 10,
                        QuantityPDRStock = 0,
                        IrreparableQuantity = 0,
                        Repairing_Quantity = 0,
                        Warranty = DateTime.Now.AddYears(1)
                    },
                    new Materiel
                    {
                        MaterielName = "Logitech MX Master 3 Mouse",
                        Type = "Peripheral",
                        SerialNumber = "LMX3-001",
                        QuantityITStock = 8,
                        QuantityPDRStock = 0,
                        IrreparableQuantity = 0,
                        Repairing_Quantity = 0,
                        Warranty = DateTime.Now.AddYears(2)
                    }
                };

                context.Materiels.AddRange(materials);
                await context.SaveChangesAsync(cancellationToken);
            }

            // Assignments
            if (!await context.Assignments.AnyAsync(cancellationToken))
            {
                var admin = await context.Employees.FirstOrDefaultAsync(e => e.Email == "admin@asteelflash.com", cancellationToken);
                var john = await context.Employees.FirstOrDefaultAsync(e => e.Email == "john.smith@asteelflash.com", cancellationToken);
                var project = await context.Projects.OrderBy(p => p.Id).FirstOrDefaultAsync(cancellationToken);

                if (admin != null && john != null && project != null)
                {
                    var assignment = new Assignment
                    {
                        AssignedTo = john.Id,
                        AssignedBy = admin.Id,
                        ProjectId = project.Id,
                        Date = DateTime.Now.AddDays(-30),
                        Descipriton = "Laptop assignment for development work",
                        OnMission = true,
                        RestoreDateLimit = DateTime.Now.AddDays(30)
                    };
                    context.Assignments.Add(assignment);
                    await context.SaveChangesAsync(cancellationToken);

                    var laptop = await context.Materiels.FirstOrDefaultAsync(m => m.Type == "Laptop", cancellationToken);
                    if (laptop != null)
                    {
                        context.AssignmentMateriels.Add(new AssignmentMateriel
                        {
                            AssignmentId = assignment.Id,
                            MaterielId = laptop.Id,
                            Qte = 1
                        });
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            // Requests
            if (!await context.Requests.AnyAsync(cancellationToken))
            {
                var sarah = await context.Employees.FirstOrDefaultAsync(e => e.Email == "sarah.johnson@asteelflash.com", cancellationToken);
                var mike = await context.Employees.FirstOrDefaultAsync(e => e.Email == "mike.davis@asteelflash.com", cancellationToken);

                if (sarah != null && mike != null)
                {
                    context.Requests.AddRange(
                        new Request
                        {
                            EmployeeId = sarah.Id,
                            Title = "Additional Monitors Request",
                            ProjectName = "Website Redesign",
                            Description = "Need additional monitors for design team",
                            MaterialType = "Monitors",
                            Date = DateTime.Now.AddDays(-15),
                            Status = "Approved",
                            File = Array.Empty<byte>()
                        },
                        new Request
                        {
                            EmployeeId = mike.Id,
                            Title = "Keyboard and Mouse Request",
                            ProjectName = "IT Support",
                            Description = "Request for new keyboard and mouse",
                            MaterialType = "Peripherals",
                            Date = DateTime.Now.AddDays(-7),
                            Status = "Pending",
                            File = Array.Empty<byte>()
                        });

                    await context.SaveChangesAsync(cancellationToken);
                }
            }

            // Delivery Orders
            if (!await context.DeliveryOrders.AnyAsync(cancellationToken))
            {
                var supplier = await context.Suppliers.FirstOrDefaultAsync(cancellationToken);
                var mike = await context.Employees.FirstOrDefaultAsync(e => e.Email == "mike.davis@asteelflash.com", cancellationToken);

                if (supplier != null && mike != null)
                {
                    var deliveryOrder = new DeliveryOrder
                    {
                        DeleveryOrderNumber = "DO-2024-001",
                        OrderNumber = "ORD-2024-001",
                        Descriptoin = "Office supplies and peripherals",
                        SupplierName = supplier.SupplierName,
                        Date = DateTime.Now.AddDays(-20),
                        DeliveryDate = DateTime.Now.AddDays(-10),
                        EmployeeId = mike.Id,
                        HasDelayedM = false
                    };
                    context.DeliveryOrders.Add(deliveryOrder);
                    await context.SaveChangesAsync(cancellationToken);

                    var mouse = await context.Materiels.FirstOrDefaultAsync(m => m.Type == "Peripheral", cancellationToken);
                    if (mouse != null)
                    {
                        context.DeliveryOrderMateriels.Add(new DeliveryOrderMateriel
                        {
                            DeliveryOrderNumber = deliveryOrder.DeleveryOrderNumber,
                            MaterielId = mouse.Id,
                            Qte = 5
                        });
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            logger.LogInformation("Database seeding complete.");
        }

        private static async Task ClearOldDataAsync(
            ITStockManagmentContext context,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            // Destructive, intended for developer convenience only.
            using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

            await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Offer]", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[DeliveryOrderMateriel]", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[DeliveryOrder]", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[AssignmentMateriel]", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Assignment]", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Request]", cancellationToken);

            await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Materiel]", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Supplier]", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Project]", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Employee]", cancellationToken);

            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[dbo].[Employee]', RESEED, 0)", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[dbo].[Project]', RESEED, 0)", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[dbo].[Materiel]', RESEED, 0)", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[dbo].[Assignment]', RESEED, 0)", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[dbo].[Request]', RESEED, 0)", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[dbo].[Offer]', RESEED, 0)", cancellationToken);

            await tx.CommitAsync(cancellationToken);
            logger.LogInformation("Cleared old data (development-only).");
        }
    }
}
