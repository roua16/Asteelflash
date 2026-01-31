using ITStockM.Components;
using ITStockM.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Radzen;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);
builder.Services.AddControllers();
builder.Services.AddRadzenComponents();

builder.Services.AddHttpClient();
builder.Services.AddScoped<ITStockManagmentService>();

builder.Services.AddDbContext<ITStockM.Data.ITStockManagmentContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ITStockManagmentConnection"));
});



builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "IT";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";

        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);



    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
builder.Services.AddAuthorization();
builder.Services.AddSingleton<EmailService>(new EmailService(
    smtpServer: "smtp.office365.com",
    smtpPort: 587,
    fromEmail: "griramed@hotmail.com",
    password: "ylpysozvumgdtfgb"
));

builder.Services.AddHostedService<EmailBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ITStockM.Data.ITStockManagmentContext>();
        context.Database.Migrate();
        Console.WriteLine("✔️ Migration exécutée avec succès.");

        // Clear old data to avoid FK/seeding issues and reset identity seeds, but only in Development to avoid accidental data loss in production
        if (app.Environment.IsDevelopment())
        {
            await ClearOldDataAsync(context);
            Console.WriteLine("🧹 Anciennes données supprimées avec succès.");
        }
        else
        {
            Console.WriteLine("ℹ️ Skipping data clear on non-development environment. To enable, set environment to Development or add a startup flag.");
        }

        // Seed data
        await SeedDataAsync(context);
        Console.WriteLine("✔️ Données de départ ajoutées avec succès.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Erreur : " + ex.Message);
    }
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

// Helper: Clear old data safely before seeding to avoid FK constraint errors and NULL insert issues
static async Task ClearOldDataAsync(ITStockM.Data.ITStockManagmentContext context)
{
    try
    {
        // Use a transaction so everything is cleared atomically
        using var tx = await context.Database.BeginTransactionAsync();

        // Delete dependent / child tables first to avoid FK constraint violations
        await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Offer]");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[DeliveryOrderMateriel]");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[DeliveryOrder]");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[AssignmentMateriel]");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Assignment]");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Request]");

        // Now delete independent tables
        await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Materiel]");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Supplier]");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Project]");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Employee]");

        // Reset identity seeds for tables that use identity columns so inserts won't conflict
        await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[dbo].[Employee]', RESEED, 0)");
        await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[dbo].[Project]', RESEED, 0)");
        await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[dbo].[Materiel]', RESEED, 0)");
        await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[dbo].[Assignment]', RESEED, 0)");
        await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[dbo].[Request]', RESEED, 0)");
        await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[dbo].[Offer]', RESEED, 0)");

        await tx.CommitAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Erreur lors de la suppression des anciennes données : " + ex.Message);
        // Rethrow so startup can decide what to do (we want to fail fast if clearing fails)
        throw;
    }
}

async Task SeedDataAsync(ITStockM.Data.ITStockManagmentContext context)
{
    // Seed Employees
    if (!context.Employees.Any(e => e.Email == "john.smith@asteelflash.com"))
    {
        var employees = new[]
        {
            new ITStockM.Models.ITStockManagment.Employee
            {
                FullName = "John Smith",
                Email = "john.smith@asteelflash.com",
                Password = "password123",
                Post = "Software Developer",
                PhoneNumber = "+1234567891",
                Service = "Development"
            },
            new ITStockM.Models.ITStockManagment.Employee
            {
                FullName = "Sarah Johnson",
                Email = "sarah.johnson@asteelflash.com",
                Password = "password123",
                Post = "Project Manager",
                PhoneNumber = "+1234567892",
                Service = "Management"
            },
            new ITStockM.Models.ITStockManagment.Employee
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
        await context.SaveChangesAsync();
    }

    // Seed Suppliers
    if (!context.Suppliers.Any())
    {
        var suppliers = new[]
        {
            new ITStockM.Models.ITStockManagment.Supplier
            {
                SupplierName = "TechCorp Supplies",
                Adress = "123 Tech Street, Silicon Valley, CA",
                Email = "orders@techcorp.com",
                PhoneNumber = "+1-555-0101"
            },
            new ITStockM.Models.ITStockManagment.Supplier
            {
                SupplierName = "Global IT Solutions",
                Adress = "456 Business Ave, New York, NY",
                Email = "sales@globalit.com",
                PhoneNumber = "+1-555-0102"
            },
            new ITStockM.Models.ITStockManagment.Supplier
            {
                SupplierName = "Office Equipment Plus",
                Adress = "789 Commerce Blvd, Chicago, IL",
                Email = "info@officeplus.com",
                PhoneNumber = "+1-555-0103"
            }
        };
        context.Suppliers.AddRange(suppliers);
        await context.SaveChangesAsync();
    }

    // Seed Projects
    if (!context.Projects.Any())
    {
        var projects = new[]
        {
            new ITStockM.Models.ITStockManagment.Project { ProjectName = "Website Redesign" },
            new ITStockM.Models.ITStockManagment.Project { ProjectName = "Mobile App Development" },
            new ITStockM.Models.ITStockManagment.Project { ProjectName = "Database Migration" },
            new ITStockM.Models.ITStockManagment.Project { ProjectName = "Network Infrastructure Upgrade" }
        };
        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();
    }

    // Seed Materials
    if (!context.Materiels.Any())
    {
        var materials = new[]
        {
            new ITStockM.Models.ITStockManagment.Materiel
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
            new ITStockM.Models.ITStockManagment.Materiel
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
            new ITStockM.Models.ITStockManagment.Materiel
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
            new ITStockM.Models.ITStockManagment.Materiel
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
            new ITStockM.Models.ITStockManagment.Materiel
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
        await context.SaveChangesAsync();
    }

    // Seed Assignments
    if (!context.Assignments.Any())
    {
        var admin = context.Employees.FirstOrDefault(e => e.Email == "admin@asteelflash.com");
        var john = context.Employees.FirstOrDefault(e => e.Email == "john.smith@asteelflash.com");
        var project = context.Projects.FirstOrDefault();

        if (admin != null && john != null && project != null)
        {
            var assignments = new[]
            {
                new ITStockM.Models.ITStockManagment.Assignment
                {
                    AssignedTo = john.Id,
                    AssignedBy = admin.Id,
                    ProjectId = project.Id,
                    Date = DateTime.Now.AddDays(-30),
                    Descipriton = "Laptop assignment for development work",
                    OnMission = true,
                    RestoreDateLimit = DateTime.Now.AddDays(30)
                }
            };
            context.Assignments.AddRange(assignments);
            await context.SaveChangesAsync();

            // Add assignment materials
            var assignment = context.Assignments.First();
            var laptop = context.Materiels.First(m => m.Type == "Laptop");

            var assignmentMaterials = new[]
            {
                new ITStockM.Models.ITStockManagment.AssignmentMateriel
                {
                    AssignmentId = assignment.Id,
                    MaterielId = laptop.Id,
                    Qte = 1
                }
            };
            context.AssignmentMateriels.AddRange(assignmentMaterials);
            await context.SaveChangesAsync();
        }
    }

    // Seed Requests
    if (!context.Requests.Any())
    {
        var sarah = context.Employees.FirstOrDefault(e => e.Email == "sarah.johnson@asteelflash.com");
        var mike = context.Employees.FirstOrDefault(e => e.Email == "mike.davis@asteelflash.com");

        if (sarah != null && mike != null)
        {
            var requests = new[]
            {
                new ITStockM.Models.ITStockManagment.Request
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
                new ITStockM.Models.ITStockManagment.Request
                {
                    EmployeeId = mike.Id,
                    Title = "Keyboard and Mouse Request",
                    ProjectName = "IT Support",
                    Description = "Request for new keyboard and mouse",
                    MaterialType = "Peripherals",
                    Date = DateTime.Now.AddDays(-7),
                    Status = "Pending",
                    File = Array.Empty<byte>()
                }
            };
            context.Requests.AddRange(requests);
            await context.SaveChangesAsync();
        }
    }

    // Seed Delivery Orders
    if (!context.DeliveryOrders.Any())
    {
        var supplier = context.Suppliers.FirstOrDefault();
        var mike = context.Employees.FirstOrDefault(e => e.Email == "mike.davis@asteelflash.com");

        if (supplier != null && mike != null)
        {
            var deliveryOrders = new[]
            {
                new ITStockM.Models.ITStockManagment.DeliveryOrder
                {
                    DeleveryOrderNumber = "DO-2024-001",
                    OrderNumber = "ORD-2024-001",
                    Descriptoin = "Office supplies and peripherals",
                    SupplierName = supplier.SupplierName,
                    Date = DateTime.Now.AddDays(-20),
                    DeliveryDate = DateTime.Now.AddDays(-10),
                    EmployeeId = mike.Id,
                    HasDelayedM = false
                }
            };
            context.DeliveryOrders.AddRange(deliveryOrders);
            await context.SaveChangesAsync();

            // Add delivery order materials
            var deliveryOrder = context.DeliveryOrders.First();
            var mouse = context.Materiels.First(m => m.Type == "Peripheral");

            var deliveryMaterials = new[]
            {
                new ITStockM.Models.ITStockManagment.DeliveryOrderMateriel
                {
                    DeliveryOrderNumber = deliveryOrder.DeleveryOrderNumber,
                    MaterielId = mouse.Id,
                    Qte = 5
                }
            };
            context.DeliveryOrderMateriels.AddRange(deliveryMaterials);
            await context.SaveChangesAsync();
        }
    }
}