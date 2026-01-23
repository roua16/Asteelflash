using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITStockM.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Employee",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Post = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Service = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materiel",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    materielName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuantityITStock = table.Column<int>(type: "int", nullable: false),
                    QuantityPDRStock = table.Column<int>(type: "int", nullable: false),
                    IrreparableQuantity = table.Column<int>(type: "int", nullable: false),
                    Repairing_Quantity = table.Column<int>(type: "int", nullable: false),
                    Warranty = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materiel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Supplier",
                schema: "dbo",
                columns: table => new
                {
                    SupplierName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Adress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.SupplierName);
                });

            migrationBuilder.CreateTable(
                name: "Request",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    File = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Request", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Request_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assignment",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignedTo = table.Column<int>(type: "int", nullable: true),
                    AssignedBy = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descipriton = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OnMission = table.Column<bool>(type: "bit", nullable: false),
                    RestoreDateLimit = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RestoreDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignment_Employee_AssignedBy",
                        column: x => x.AssignedBy,
                        principalSchema: "dbo",
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assignment_Employee_AssignedTo",
                        column: x => x.AssignedTo,
                        principalSchema: "dbo",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Assignment_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "dbo",
                        principalTable: "Project",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryOrder",
                schema: "dbo",
                columns: table => new
                {
                    DeleveryOrderNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descriptoin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    HasDelayedM = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryOrder", x => x.DeleveryOrderNumber);
                    table.ForeignKey(
                        name: "FK_DeliveryOrder_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryOrder_Supplier_SupplierName",
                        column: x => x.SupplierName,
                        principalSchema: "dbo",
                        principalTable: "Supplier",
                        principalColumn: "SupplierName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Offer",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Selected = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offer_Request_RequestId",
                        column: x => x.RequestId,
                        principalSchema: "dbo",
                        principalTable: "Request",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Offer_Supplier_SupplierName",
                        column: x => x.SupplierName,
                        principalSchema: "dbo",
                        principalTable: "Supplier",
                        principalColumn: "SupplierName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentMateriel",
                schema: "dbo",
                columns: table => new
                {
                    MaterielId = table.Column<int>(type: "int", nullable: false),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    Qte = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentMateriel", x => new { x.MaterielId, x.AssignmentId });
                    table.ForeignKey(
                        name: "FK_AssignmentMateriel_Assignment_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "dbo",
                        principalTable: "Assignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentMateriel_Materiel_MaterielId",
                        column: x => x.MaterielId,
                        principalSchema: "dbo",
                        principalTable: "Materiel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryOrderMateriel",
                schema: "dbo",
                columns: table => new
                {
                    MaterielId = table.Column<int>(type: "int", nullable: false),
                    DeliveryOrderNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Qte = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryOrderMateriel", x => new { x.MaterielId, x.DeliveryOrderNumber });
                    table.ForeignKey(
                        name: "FK_DeliveryOrderMateriel_DeliveryOrder_DeliveryOrderNumber",
                        column: x => x.DeliveryOrderNumber,
                        principalSchema: "dbo",
                        principalTable: "DeliveryOrder",
                        principalColumn: "DeleveryOrderNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryOrderMateriel_Materiel_MaterielId",
                        column: x => x.MaterielId,
                        principalSchema: "dbo",
                        principalTable: "Materiel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_AssignedBy",
                schema: "dbo",
                table: "Assignment",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_AssignedTo",
                schema: "dbo",
                table: "Assignment",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_ProjectId",
                schema: "dbo",
                table: "Assignment",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentMateriel_AssignmentId",
                schema: "dbo",
                table: "AssignmentMateriel",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrder_EmployeeId",
                schema: "dbo",
                table: "DeliveryOrder",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrder_SupplierName",
                schema: "dbo",
                table: "DeliveryOrder",
                column: "SupplierName");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrderMateriel_DeliveryOrderNumber",
                schema: "dbo",
                table: "DeliveryOrderMateriel",
                column: "DeliveryOrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Offer_RequestId",
                schema: "dbo",
                table: "Offer",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Offer_SupplierName",
                schema: "dbo",
                table: "Offer",
                column: "SupplierName");

            migrationBuilder.CreateIndex(
                name: "IX_Request_EmployeeId",
                schema: "dbo",
                table: "Request",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentMateriel",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DeliveryOrderMateriel",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Offer",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Assignment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DeliveryOrder",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Materiel",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Request",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Project",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Supplier",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Employee",
                schema: "dbo");
        }
    }
}
