using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDbContext<ITStockManagmentContext>(options =>
{
    options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ITStockManagmentContext-fa83d2db-5d76-4224-9c32-33e342a9e15c;Trusted_Connection=True;MultipleActiveResultSets=True");
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ITStockManagmentContext>();

var existingAdmin = await context.Employee.FirstOrDefaultAsync(e => e.Email == "admin@asteelflash.com");

if (existingAdmin != null)
{
    existingAdmin.Post = "Admin";
    await context.SaveChangesAsync();
    Console.WriteLine("Admin user updated successfully!");
}
else
{
    var admin = new Employee
    {
        FullName = "Admin User",
        Email = "admin@asteelflash.com",
        Password = "admin123",
        Post = "Admin",
        PhoneNumber = "+1234567890",
        Service = "IT"
    };

    context.Employee.Add(admin);
    await context.SaveChangesAsync();
    Console.WriteLine("Admin user created successfully!");
}

Console.WriteLine("Email: admin@asteelflash.com");
Console.WriteLine("Password: admin123");

public class ITStockManagmentContext : DbContext
{
    public ITStockManagmentContext(DbContextOptions<ITStockManagmentContext> options) : base(options) { }

    public DbSet<Employee> Employee { get; set; }
}

[Table("Employee", Schema = "dbo")]
public class Employee
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string Post { get; set; }

    [Required]
    public string PhoneNumber { get; set; }

    [Required]
    public string Service { get; set; }
}