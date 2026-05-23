using ITStockM.Domain.Entities;
using ITStockM.Domain.Enums;
using ITStockM.Models.Constants;
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
            // Use EnsureCreatedAsync for SQLite which doesn't need migrations
            await context.Database.EnsureCreatedAsync(cancellationToken);
            logger.LogInformation("Database schema ensured.");

            // Apply any column additions that EnsureCreatedAsync won't add to existing databases.
            await ApplySchemaUpdatesAsync(context, logger, cancellationToken);

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

            logger.LogInformation(
                isFirstRun
                    ? "First run detected. Starting database seeding..."
                    : "Database exists. Ensuring seed baseline data is present...");

            await SeedAdminAsync(context, seedSection, logger, cancellationToken);

            if (seedDemoData)
            {
                // Safe to run on every startup: SeedDemoDataAsync checks each table before insert.
                await SeedDemoDataAsync(context, logger, cancellationToken);
                await EnsureUiBaselineDataAsync(context, logger, cancellationToken);
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
                    Post = "Admin", // Keep for backward compatibility
                    Role = UserRoles.Admin,
                    PhoneNumber = adminPhoneNumber,
                    Service = adminService
                };

                context.Employees.Add(admin);
                await context.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Seeded admin user: {AdminEmail}", adminEmail);
            }
            else
            {
                if (!string.Equals(admin.Role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
                {
                    admin.Post = "Admin"; // Keep for backward compatibility
                    admin.Role = UserRoles.Admin;
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
                        Post = "PDR Manager",
                        Role = UserRoles.PDR,
                        PhoneNumber = "+1234567891",
                        Service = "Material Management"
                    },
                    new Employee
                    {
                        FullName = "Sarah Johnson",
                        Email = "sarah.johnson@asteelflash.com",
                        Post = "Purchasing Manager",
                        Role = UserRoles.Purchasing,
                        PhoneNumber = "+1234567892",
                        Service = "Procurement"
                    },
                    new Employee
                    {
                        FullName = "Mike Davis",
                        Email = "mike.davis@asteelflash.com",
                        Post = "IT Support Specialist",
                        Role = UserRoles.IT,
                        PhoneNumber = "+1234567893",
                        Service = "IT"
                    },
                    new Employee
                    {
                        FullName = "Alice Brown",
                        Email = "alice.brown@asteelflash.com",
                        Post = "Infrastructure Manager",
                        Role = UserRoles.Infrastructure,
                        PhoneNumber = "+1234567894",
                        Service = "Infrastructure"
                    },
                    new Employee
                    {
                        FullName = "Paul Green",
                        Email = "paul.green@asteelflash.com",
                        Post = "Purchasing Officer",
                        Role = UserRoles.Purchasing,
                        PhoneNumber = "+1234567895",
                        Service = "Procurement"
                    },
                    new Employee
                    {
                        FullName = "Emma White",
                        Email = "emma.white@asteelflash.com",
                        Post = "PDR Technician",
                        Role = UserRoles.PDR,
                        PhoneNumber = "+1234567896",
                        Service = "Material Management"
                    },
                    new Employee
                    {
                        FullName = "Thomas Miller",
                        Email = "thomas.miller@asteelflash.com",
                        Post = "Employee",
                        Role = UserRoles.Employee,
                        PhoneNumber = "+1234567897",
                        Service = "General"
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
                    },
                    new Supplier
                    {
                        SupplierName = "NetSupply Co.",
                        Adress = "12 Harbor Rd, Boston, MA",
                        Email = "contact@netsupply.co",
                        PhoneNumber = "+1-555-0201"
                    },
                    new Supplier
                    {
                        SupplierName = "AlphaTech Distributors",
                        Adress = "88 Industrial Park, Austin, TX",
                        Email = "sales@alphatech.com",
                        PhoneNumber = "+1-555-0302"
                    },
                    new Supplier
                    {
                        SupplierName = "SecureParts Ltd.",
                        Adress = "9 Secure Ln, Seattle, WA",
                        Email = "orders@secureparts.com",
                        PhoneNumber = "+1-555-0403"
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
                    },

                    // Additional demo materials
                    new Materiel
                    {
                        MaterielName = "Dell 24\" Monitor (FHD)",
                        Type = "Monitor",
                        SerialNumber = "MON24-001",
                        QuantityITStock = 12,
                        QuantityPDRStock = 3,
                        IrreparableQuantity = 0,
                        Repairing_Quantity = 0,
                        Warranty = DateTime.Now.AddYears(2)
                    },
                    new Materiel
                    {
                        MaterielName = "USB-C Docking Station",
                        Type = "Docking Station",
                        SerialNumber = "DOC-100",
                        QuantityITStock = 6,
                        QuantityPDRStock = 1,
                        IrreparableQuantity = 0,
                        Repairing_Quantity = 0,
                        Warranty = DateTime.Now.AddYears(1)
                    },
                    new Materiel
                    {
                        MaterielName = "Samsung 1TB NVMe SSD",
                        Type = "Storage",
                        SerialNumber = "SSD1TB-001",
                        QuantityITStock = 10,
                        QuantityPDRStock = 0,
                        IrreparableQuantity = 0,
                        Repairing_Quantity = 0,
                        Warranty = DateTime.Now.AddYears(3)
                    },
                    new Materiel
                    {
                        MaterielName = "Logitech C920 Webcam",
                        Type = "Peripheral",
                        SerialNumber = "C920-001",
                        QuantityITStock = 7,
                        QuantityPDRStock = 0,
                        IrreparableQuantity = 0,
                        Repairing_Quantity = 0,
                        Warranty = DateTime.Now.AddYears(2)
                    },
                    new Materiel
                    {
                        MaterielName = "Jabra Evolve 40 Headset",
                        Type = "Peripheral",
                        SerialNumber = "JAB-E40-001",
                        QuantityITStock = 5,
                        QuantityPDRStock = 0,
                        IrreparableQuantity = 0,
                        Repairing_Quantity = 0,
                        Warranty = DateTime.Now.AddYears(2)
                    },
                    new Materiel
                    {
                        MaterielName = "Cat6 Ethernet Cable (1m)",
                        Type = "Consumable",
                        SerialNumber = null,
                        QuantityITStock = 50,
                        QuantityPDRStock = 100,
                        IrreparableQuantity = 0,
                        Repairing_Quantity = 0,
                        Warranty = DateTime.Now.AddYears(1)
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
                var thomas = await context.Employees.FirstOrDefaultAsync(e => e.Email == "thomas.miller@asteelflash.com", cancellationToken);
                var emma = await context.Employees.FirstOrDefaultAsync(e => e.Email == "emma.white@asteelflash.com", cancellationToken);
                var paul = await context.Employees.FirstOrDefaultAsync(e => e.Email == "paul.green@asteelflash.com", cancellationToken);
                var project = await context.Projects.OrderBy(p => p.Id).FirstOrDefaultAsync(cancellationToken);

                if (admin != null && john != null && project != null)
                {
                    // Active assignment (existing scenario)
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

                    // Recently completed / archived assignment for Emma (Archived mission)
                    var archivedAssignment = new Assignment
                    {
                        AssignedTo = emma != null ? emma.Id : john.Id,
                        AssignedBy = admin.Id,
                        ProjectId = project.Id,
                        Date = DateTime.Now.AddDays(-45),
                        Descipriton = "Returned equipment after PDR task",
                        OnMission = false,
                        RestoreDate = DateTime.Now.AddDays(-5),
                        RestoreDateLimit = DateTime.Now.AddDays(-10)
                    };
                    context.Assignments.Add(archivedAssignment);

                    // On-mission assignment with near deadline for Paul
                    var urgentAssignment = new Assignment
                    {
                        AssignedTo = paul != null ? paul.Id : john.Id,
                        AssignedBy = admin.Id,
                        ProjectId = project.Id,
                        Date = DateTime.Now.AddDays(-8),
                        Descipriton = "Field mission - temporary devices",
                        OnMission = true,
                        RestoreDateLimit = DateTime.Now.AddDays(3)
                    };
                    context.Assignments.Add(urgentAssignment);

                    await context.SaveChangesAsync(cancellationToken);

                    var laptop = await context.Materiels.FirstOrDefaultAsync(m => m.Type == "Laptop", cancellationToken);
                    var mouse = await context.Materiels.FirstOrDefaultAsync(m => m.MaterielName.Contains("Mouse"), cancellationToken);
                    var printer = await context.Materiels.FirstOrDefaultAsync(m => m.Type == "Printer", cancellationToken);
                    var monitor = await context.Materiels.FirstOrDefaultAsync(m => m.Type == "Monitor", cancellationToken);

                    if (laptop != null)
                    {
                        context.AssignmentMateriels.Add(new AssignmentMateriel
                        {
                            AssignmentId = assignment.Id,
                            MaterielId = laptop.Id,
                            Qte = 1
                        });
                    }

                    if (printer != null)
                    {
                        context.AssignmentMateriels.Add(new AssignmentMateriel
                        {
                            AssignmentId = archivedAssignment.Id,
                            MaterielId = printer.Id,
                            Qte = 1
                        });
                    }

                    if (monitor != null)
                    {
                        context.AssignmentMateriels.Add(new AssignmentMateriel
                        {
                            AssignmentId = urgentAssignment.Id,
                            MaterielId = monitor.Id,
                            Qte = 2
                        });
                    }

                    if (mouse != null && thomas != null)
                    {
                        // small assignment for Thomas
                        var personalAssignment = new Assignment
                        {
                            AssignedTo = thomas.Id,
                            AssignedBy = admin.Id,
                            ProjectId = project.Id,
                            Date = DateTime.Now.AddDays(-12),
                            Descipriton = "Peripheral allocation",
                            OnMission = false,
                            RestoreDateLimit = null
                        };
                        context.Assignments.Add(personalAssignment);
                        await context.SaveChangesAsync(cancellationToken);

                        context.AssignmentMateriels.Add(new AssignmentMateriel
                        {
                            AssignmentId = personalAssignment.Id,
                            MaterielId = mouse.Id,
                            Qte = 1
                        });
                    }

                    await context.SaveChangesAsync(cancellationToken);
                }

                // Add a sample assignment that simulates a partial/damaged return (for testing notifications)
                var partialReturnThomas = await context.Employees.FirstOrDefaultAsync(e => e.Email == "thomas.miller@asteelflash.com", cancellationToken);
                var laptopMat = await context.Materiels.FirstOrDefaultAsync(m => m.Type == "Laptop", cancellationToken);
                if (partialReturnThomas != null && laptopMat != null && project != null && !await context.Assignments.AnyAsync(a => a.Descipriton.Contains("Partial return example"), cancellationToken))
                {
                    var problemAssignment = new Assignment
                    {
                        AssignedTo = partialReturnThomas.Id,
                        AssignedBy = admin != null ? admin.Id : partialReturnThomas.Id,
                        ProjectId = project.Id,
                        Date = DateTime.Now.AddDays(-40),
                        Descipriton = "Partial return example: 2 laptops assigned, 1 returned damaged",
                        OnMission = false,
                        RestoreDate = DateTime.Now.AddDays(-5),
                        RestoreDateLimit = DateTime.Now.AddDays(-10)
                    };
                    context.Assignments.Add(problemAssignment);
                    await context.SaveChangesAsync(cancellationToken);

                    context.AssignmentMateriels.Add(new AssignmentMateriel
                    {
                        AssignmentId = problemAssignment.Id,
                        MaterielId = laptopMat.Id,
                        Qte = 1 // 1 still outstanding (was 2 originally)
                    });
                    await context.SaveChangesAsync(cancellationToken);
                }
            }

            // Requests
            if (!await context.Requests.AnyAsync(cancellationToken))
            {
                var sarah = await context.Employees.FirstOrDefaultAsync(e => e.Email == "sarah.johnson@asteelflash.com", cancellationToken);
                var mike = await context.Employees.FirstOrDefaultAsync(e => e.Email == "mike.davis@asteelflash.com", cancellationToken);
                var paul = await context.Employees.FirstOrDefaultAsync(e => e.Email == "paul.green@asteelflash.com", cancellationToken);
                var emma = await context.Employees.FirstOrDefaultAsync(e => e.Email == "emma.white@asteelflash.com", cancellationToken);

                if (sarah != null && mike != null)
                {
                    // Core requests
                    context.Requests.AddRange(
                        new Request
                        {
                            EmployeeId = sarah.Id,
                            Title = "Additional Monitors Request",
                            ProjectName = "Website Redesign",
                            Description = "Need additional monitors for design team",
                            MaterialType = "Monitor",
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
                            MaterialType = "Peripheral",
                            Date = DateTime.Now.AddDays(-7),
                            Status = "Pending",
                            File = Array.Empty<byte>()
                        },

                        // Additional demo requests to populate Pending/Archived views
                        new Request
                        {
                            EmployeeId = paul != null ? paul.Id : sarah.Id,
                            Title = "Ergonomic Chairs Request",
                            ProjectName = "Office Ergonomics",
                            Description = "Purchase of 5 ergonomic chairs",
                            MaterialType = "Other",
                            Date = DateTime.Now.AddDays(-40),
                            Status = "Done",
                            File = Array.Empty<byte>()
                        },
                        new Request
                        {
                            EmployeeId = emma != null ? emma.Id : sarah.Id,
                            Title = "Spare Batteries Request",
                            ProjectName = "PDR Stock",
                            Description = "Spare batteries for hand tools",
                            MaterialType = "Other",
                            Date = DateTime.Now.AddDays(-20),
                            Status = "Approved",
                            File = Array.Empty<byte>()
                        });

                    await context.SaveChangesAsync(cancellationToken);

                    // Create offers for some requests (selected/unselected, past/future delivery dates)
                    var monitorsRequest = await context.Requests.FirstOrDefaultAsync(r => r.Title == "Additional Monitors Request", cancellationToken);
                    var keyboardRequest = await context.Requests.FirstOrDefaultAsync(r => r.Title == "Keyboard and Mouse Request", cancellationToken);
                    var chairsRequest = await context.Requests.FirstOrDefaultAsync(r => r.Title == "Ergonomic Chairs Request", cancellationToken);

                    var supplier1 = await context.Suppliers.FirstOrDefaultAsync(s => s.SupplierName == "TechCorp Supplies", cancellationToken);
                    var supplier2 = await context.Suppliers.FirstOrDefaultAsync(s => s.SupplierName == "Global IT Solutions", cancellationToken);

                    if (monitorsRequest != null && supplier1 != null)
                    {
                        context.Offers.Add(new Offer
                        {
                            RequestId = monitorsRequest.Id,
                            SupplierName = supplier1.SupplierName,
                            DeliveryDate = DateTime.Now.AddDays(5),
                            Price = 1200m,
                            Selected = true
                        });

                        context.Offers.Add(new Offer
                        {
                            RequestId = monitorsRequest.Id,
                            SupplierName = supplier2 != null ? supplier2.SupplierName : "Office Equipment Plus",
                            DeliveryDate = DateTime.Now.AddDays(7),
                            Price = 1350m,
                            Selected = false
                        });
                    }

                    if (keyboardRequest != null && supplier2 != null)
                    {
                        // a past delivery -> should appear in Archived Requests
                        context.Offers.Add(new Offer
                        {
                            RequestId = keyboardRequest.Id,
                            SupplierName = supplier2.SupplierName,
                            DeliveryDate = DateTime.Now.AddDays(-3),
                            Price = 150m,
                            Selected = true
                        });
                    }

                    if (chairsRequest != null && supplier1 != null)
                    {
                        context.Offers.Add(new Offer
                        {
                            RequestId = chairsRequest.Id,
                            SupplierName = supplier1.SupplierName,
                            DeliveryDate = DateTime.Now.AddDays(-20),
                            Price = 2500m,
                            Selected = true
                        });
                    }

                    await context.SaveChangesAsync(cancellationToken);
                }
            }

            // Delivery Orders
            if (!await context.DeliveryOrders.AnyAsync(cancellationToken))
            {
                var supplier = await context.Suppliers.FirstOrDefaultAsync(cancellationToken);
                var mike = await context.Employees.FirstOrDefaultAsync(e => e.Email == "mike.davis@asteelflash.com", cancellationToken);
                var admin = await context.Employees.FirstOrDefaultAsync(e => e.Email == "admin@asteelflash.com", cancellationToken);

                if (supplier != null && mike != null)
                {
                    var deliveryOrder1 = new DeliveryOrder
                    {
                        DeliveryOrderNumber = "DO-2024-001",
                        OrderNumber = "ORD-2024-001",
                        Descriptoin = "Office supplies and peripherals",
                        SupplierName = supplier.SupplierName,
                        Date = DateTime.Now.AddDays(-20),
                        DeliveryDate = DateTime.Now.AddDays(-10),
                        EmployeeId = mike.Id,
                        HasDelayedM = false
                    };
                    context.DeliveryOrders.Add(deliveryOrder1);

                    var deliveryOrder2 = new DeliveryOrder
                    {
                        DeliveryOrderNumber = "DO-2025-002",
                        OrderNumber = "ORD-2025-002",
                        Descriptoin = "Monitors and docking stations",
                        SupplierName = supplier.SupplierName,
                        Date = DateTime.Now.AddDays(-5),
                        DeliveryDate = DateTime.Now.AddDays(2),
                        EmployeeId = admin != null ? admin.Id : mike.Id,
                        HasDelayedM = false
                    };
                    context.DeliveryOrders.Add(deliveryOrder2);

                    var deliveryOrder3 = new DeliveryOrder
                    {
                        DeliveryOrderNumber = "DO-2023-010",
                        OrderNumber = "ORD-2023-010",
                        Descriptoin = "Old delivery (archived)",
                        SupplierName = supplier.SupplierName,
                        Date = DateTime.Now.AddYears(-1),
                        DeliveryDate = DateTime.Now.AddYears(-1).AddDays(3),
                        EmployeeId = mike.Id,
                        HasDelayedM = false
                    };
                    context.DeliveryOrders.Add(deliveryOrder3);

                    await context.SaveChangesAsync(cancellationToken);

                    var mouse = await context.Materiels.FirstOrDefaultAsync(m => m.MaterielName.Contains("Mouse"), cancellationToken);
                    var monitor = await context.Materiels.FirstOrDefaultAsync(m => m.Type == "Monitor", cancellationToken);
                    var dock = await context.Materiels.FirstOrDefaultAsync(m => m.Type == "Docking Station", cancellationToken);
                    var ssd = await context.Materiels.FirstOrDefaultAsync(m => m.Type == "Storage", cancellationToken);

                    if (mouse != null)
                    {
                        context.DeliveryOrderMateriels.Add(new DeliveryOrderMateriel
                        {
                            DeliveryOrderNumber = deliveryOrder1.DeliveryOrderNumber,
                            MaterielId = mouse.Id,
                            Qte = 5
                        });
                    }

                    if (monitor != null)
                    {
                        context.DeliveryOrderMateriels.Add(new DeliveryOrderMateriel
                        {
                            DeliveryOrderNumber = deliveryOrder2.DeliveryOrderNumber,
                            MaterielId = monitor.Id,
                            Qte = 6
                        });
                    }

                    if (dock != null)
                    {
                        context.DeliveryOrderMateriels.Add(new DeliveryOrderMateriel
                        {
                            DeliveryOrderNumber = deliveryOrder2.DeliveryOrderNumber,
                            MaterielId = dock.Id,
                            Qte = 4
                        });
                    }

                    if (ssd != null)
                    {
                        context.DeliveryOrderMateriels.Add(new DeliveryOrderMateriel
                        {
                            DeliveryOrderNumber = deliveryOrder3.DeliveryOrderNumber,
                            MaterielId = ssd.Id,
                            Qte = 10
                        });
                    }

                    await context.SaveChangesAsync(cancellationToken);
                }
            }

            logger.LogInformation("Database seeding complete.");
        }

        private static async Task EnsureUiBaselineDataAsync(
            ITStockManagmentContext context,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var hasChanges = false;

            var requiredEmployees = new[]
            {
                new Employee
                {
                    FullName = "John Smith",
                    Email = "john.smith@asteelflash.com",
                    Post = "PDR Manager",
                    Role = UserRoles.PDR,
                    PhoneNumber = "+1234567891",
                    Service = "Material Management"
                },
                new Employee
                {
                    FullName = "Sarah Johnson",
                    Email = "sarah.johnson@asteelflash.com",
                    Post = "Purchasing Manager",
                    Role = UserRoles.Purchasing,
                    PhoneNumber = "+1234567892",
                    Service = "Procurement"
                },
                new Employee
                {
                    FullName = "Mike Davis",
                    Email = "mike.davis@asteelflash.com",
                    Post = "IT Support Specialist",
                    Role = UserRoles.IT,
                    PhoneNumber = "+1234567893",
                    Service = "IT"
                },
                new Employee
                {
                    FullName = "Alice Brown",
                    Email = "alice.brown@asteelflash.com",
                    Post = "Infrastructure Manager",
                    Role = UserRoles.Infrastructure,
                    PhoneNumber = "+1234567894",
                    Service = "Infrastructure"
                },
                new Employee
                {
                    FullName = "Thomas Miller",
                    Email = "thomas.miller@asteelflash.com",
                    Post = "Employee",
                    Role = UserRoles.Employee,
                    PhoneNumber = "+1234567897",
                    Service = "General"
                }
            };

            foreach (var employee in requiredEmployees)
            {
                var exists = await context.Employees
                    .AnyAsync(e => e.Email == employee.Email, cancellationToken);
                if (!exists)
                {
                    context.Employees.Add(employee);
                    hasChanges = true;
                }
            }

            var requiredProjects = new[]
            {
                "Website Redesign",
                "Mobile App Development",
                "Database Migration",
                "Network Infrastructure Upgrade"
            };
            foreach (var projectName in requiredProjects)
            {
                var exists = await context.Projects
                    .AnyAsync(p => p.ProjectName == projectName, cancellationToken);
                if (!exists)
                {
                    context.Projects.Add(new Project { ProjectName = projectName });
                    hasChanges = true;
                }
            }

            var requiredSuppliers = new[]
            {
                ("TechCorp Supplies", "orders@techcorp.com"),
                ("Global IT Solutions", "sales@globalit.com"),
                ("Office Equipment Plus", "info@officeplus.com")
            };
            foreach (var (supplierName, supplierEmail) in requiredSuppliers)
            {
                var exists = await context.Suppliers
                    .AnyAsync(s => s.Email == supplierEmail || s.SupplierName == supplierName, cancellationToken);
                if (!exists)
                {
                    context.Suppliers.Add(new Supplier
                    {
                        SupplierName = supplierName,
                        Email = supplierEmail,
                        Adress = "Seed baseline address",
                        PhoneNumber = "+1-555-0000"
                    });
                    hasChanges = true;
                }
            }

            var requiredMaterials = new[]
            {
                new Materiel
                {
                    MaterielName = "Dell Latitude 5420 Laptop",
                    Type = "Laptop",
                    SerialNumber = "DL5420-001",
                    QuantityITStock = 12,
                    QuantityPDRStock = 4,
                    IrreparableQuantity = 0,
                    Repairing_Quantity = 1,
                    Warranty = DateTime.UtcNow.AddYears(2),
                    PurchaseDate = DateTime.UtcNow.AddMonths(-8),
                    ExpectedLifetimeMonths = 48,
                    LifecycleStatus = LifecycleStage.InStock
                },
                new Materiel
                {
                    MaterielName = "Lenovo ThinkCentre M70",
                    Type = "Desktop",
                    SerialNumber = "LNV-M70-015",
                    QuantityITStock = 8,
                    QuantityPDRStock = 3,
                    IrreparableQuantity = 0,
                    Repairing_Quantity = 0,
                    Warranty = DateTime.UtcNow.AddYears(1),
                    PurchaseDate = DateTime.UtcNow.AddMonths(-12),
                    ExpectedLifetimeMonths = 60,
                    LifecycleStatus = LifecycleStage.InStock
                },
                new Materiel
                {
                    MaterielName = "HP Monitor 24\"",
                    Type = "Monitor",
                    SerialNumber = "HPM24-021",
                    QuantityITStock = 20,
                    QuantityPDRStock = 6,
                    IrreparableQuantity = 1,
                    Repairing_Quantity = 1,
                    Warranty = DateTime.UtcNow.AddMonths(9),
                    PurchaseDate = DateTime.UtcNow.AddMonths(-20),
                    ExpectedLifetimeMonths = 72,
                    LifecycleStatus = LifecycleStage.InStock
                }
            };

            foreach (var material in requiredMaterials)
            {
                var exists = await context.Materiels
                    .AnyAsync(m => m.SerialNumber == material.SerialNumber, cancellationToken);
                if (!exists)
                {
                    context.Materiels.Add(material);
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await context.SaveChangesAsync(cancellationToken);
            }

            if (!await context.Assignments.AnyAsync(cancellationToken))
            {
                var admin = await context.Employees.FirstOrDefaultAsync(e => e.Email == "admin@asteelflash.com", cancellationToken);
                var assignedTo = await context.Employees.FirstOrDefaultAsync(e => e.Role == UserRoles.Employee, cancellationToken);
                var project = await context.Projects.OrderBy(p => p.Id).FirstOrDefaultAsync(cancellationToken);
                var material = await context.Materiels.OrderBy(m => m.Id).FirstOrDefaultAsync(cancellationToken);

                if (admin != null && assignedTo != null && project != null)
                {
                    var assignment = new Assignment
                    {
                        AssignedTo = assignedTo.Id,
                        AssignedBy = admin.Id,
                        ProjectId = project.Id,
                        Date = DateTime.UtcNow.AddDays(-7),
                        Descipriton = "Baseline seeded assignment for UI flows",
                        OnMission = true,
                        RestoreDateLimit = DateTime.UtcNow.AddDays(30)
                    };

                    context.Assignments.Add(assignment);
                    await context.SaveChangesAsync(cancellationToken);

                    if (material != null)
                    {
                        context.AssignmentMateriels.Add(new AssignmentMateriel
                        {
                            AssignmentId = assignment.Id,
                            MaterielId = material.Id,
                            Qte = 1
                        });
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            if (!await context.AssetLifecycleRecords.AnyAsync(cancellationToken))
            {
                var materials = await context.Materiels.ToListAsync(cancellationToken);
                foreach (var material in materials)
                {
                    context.AssetLifecycleRecords.Add(new AssetLifecycleRecord
                    {
                        MaterielId = material.Id,
                        Stage = string.IsNullOrWhiteSpace(material.LifecycleStatus)
                            ? LifecycleStage.InStock
                            : material.LifecycleStatus,
                        StartDate = DateTime.UtcNow.AddDays(-30),
                        Notes = "Seed baseline lifecycle stage"
                    });
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            logger.LogInformation("Baseline UI seed consistency check complete.");
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

        /// <summary>
        /// Idempotent schema updates: adds columns that EnsureCreatedAsync won't add to existing databases.
        /// Safe to run on every startup.
        /// </summary>
        private static async Task ApplySchemaUpdatesAsync(
            ITStockManagmentContext context,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                // Add SupplierId FK column to Materiel if it doesn't exist (added in v5)
                await context.Database.ExecuteSqlRawAsync("""
                    IF NOT EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = 'Materiel' AND COLUMN_NAME = 'SupplierId'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[Materiel] ADD [SupplierId] INT NULL;
                        ALTER TABLE [dbo].[Materiel]
                            ADD CONSTRAINT [FK_Materiel_Supplier_SupplierId]
                            FOREIGN KEY ([SupplierId]) REFERENCES [dbo].[Supplier] ([Id])
                            ON DELETE SET NULL;
                        CREATE INDEX [IX_Materiel_SupplierId] ON [dbo].[Materiel] ([SupplierId]);
                    END
                    """, cancellationToken);

                // Add IsProvisional column to Assignment if it doesn't exist (added in v5)
                await context.Database.ExecuteSqlRawAsync("""
                    IF NOT EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = 'Assignment' AND COLUMN_NAME = 'IsProvisional'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[Assignment] ADD [IsProvisional] BIT NOT NULL DEFAULT 0;
                    END
                    """, cancellationToken);

                logger.LogInformation("Schema updates applied.");
            }
            catch (Exception ex)
            {
                // SQLite doesn't support IF NOT EXISTS in ALTER TABLE; skip gracefully.
                logger.LogWarning(ex, "Schema update step skipped (may be running SQLite or column already present).");
            }
        }
    }
}
