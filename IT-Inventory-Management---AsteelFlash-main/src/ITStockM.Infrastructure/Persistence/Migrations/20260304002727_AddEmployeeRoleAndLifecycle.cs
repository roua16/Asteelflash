using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITStockM.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeRoleAndLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentHealthScore",
                schema: "dbo",
                table: "Materiel",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExpectedLifetimeMonths",
                schema: "dbo",
                table: "Materiel",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LifecycleStatus",
                schema: "dbo",
                table: "Materiel",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                schema: "dbo",
                table: "Materiel",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                schema: "dbo",
                table: "Employee",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AssetLifecycleRecord",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterielId = table.Column<int>(type: "int", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetLifecycleRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetLifecycleRecord_Materiel_MaterielId",
                        column: x => x.MaterielId,
                        principalSchema: "dbo",
                        principalTable: "Materiel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetPrediction",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterielId = table.Column<int>(type: "int", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HealthScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    HealthStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PredictedFailureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecommendedReplacementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecommendationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EstimatedReplacementCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetPrediction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetPrediction_Materiel_MaterielId",
                        column: x => x.MaterielId,
                        principalSchema: "dbo",
                        principalTable: "Materiel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceTicket",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterielId = table.Column<int>(type: "int", nullable: false),
                    ProblemDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ReportedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceTicket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceTicket_Employee_ReportedByEmployeeId",
                        column: x => x.ReportedByEmployeeId,
                        principalSchema: "dbo",
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MaintenanceTicket_Materiel_MaterielId",
                        column: x => x.MaterielId,
                        principalSchema: "dbo",
                        principalTable: "Materiel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetLifecycleRecord_MaterielId",
                schema: "dbo",
                table: "AssetLifecycleRecord",
                column: "MaterielId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetPrediction_MaterielId",
                schema: "dbo",
                table: "AssetPrediction",
                column: "MaterielId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTicket_MaterielId",
                schema: "dbo",
                table: "MaintenanceTicket",
                column: "MaterielId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTicket_ReportedByEmployeeId",
                schema: "dbo",
                table: "MaintenanceTicket",
                column: "ReportedByEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetLifecycleRecord",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AssetPrediction",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MaintenanceTicket",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "CurrentHealthScore",
                schema: "dbo",
                table: "Materiel");

            migrationBuilder.DropColumn(
                name: "ExpectedLifetimeMonths",
                schema: "dbo",
                table: "Materiel");

            migrationBuilder.DropColumn(
                name: "LifecycleStatus",
                schema: "dbo",
                table: "Materiel");

            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                schema: "dbo",
                table: "Materiel");

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "dbo",
                table: "Employee");
        }
    }
}
