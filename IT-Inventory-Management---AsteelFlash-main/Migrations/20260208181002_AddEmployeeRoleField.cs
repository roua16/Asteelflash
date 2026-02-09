using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITStockM.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeRoleField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                schema: "dbo",
                table: "Employee",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                schema: "dbo",
                table: "Employee");
        }
    }
}
