using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialCreatenew5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PointSetups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtendedToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAllCustomer = table.Column<bool>(type: "bit", nullable: false),
                    NotifyBeforeDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PointSetupCustomers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointSetupCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointSetupCustomers_PointSetups_FatherId",
                        column: x => x.FatherId,
                        principalTable: "PointSetups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PointSetupLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    Point = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointSetupLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointSetupLines_PointSetups_FatherId",
                        column: x => x.FatherId,
                        principalTable: "PointSetups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PointSetupBrands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointSetupBrands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointSetupBrands_Brand_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PointSetupBrands_PointSetupLines_FatherId",
                        column: x => x.FatherId,
                        principalTable: "PointSetupLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PointSetupIndustries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    IndustryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointSetupIndustries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointSetupIndustries_Industry_IndustryId",
                        column: x => x.IndustryId,
                        principalTable: "Industry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PointSetupIndustries_PointSetupLines_FatherId",
                        column: x => x.FatherId,
                        principalTable: "PointSetupLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PointSetupItemTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ItemName = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointSetupItemTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointSetupItemTypes_PointSetupLines_FatherId",
                        column: x => x.FatherId,
                        principalTable: "PointSetupLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PointSetupPackings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    PackingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointSetupPackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointSetupPackings_Packing_PackingId",
                        column: x => x.PackingId,
                        principalTable: "Packing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PointSetupPackings_PointSetupLines_FatherId",
                        column: x => x.FatherId,
                        principalTable: "PointSetupLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PointSetupBrands_BrandId",
                table: "PointSetupBrands",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_PointSetupBrands_FatherId",
                table: "PointSetupBrands",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_PointSetupCustomers_FatherId",
                table: "PointSetupCustomers",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_PointSetupIndustries_FatherId",
                table: "PointSetupIndustries",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_PointSetupIndustries_IndustryId",
                table: "PointSetupIndustries",
                column: "IndustryId");

            migrationBuilder.CreateIndex(
                name: "IX_PointSetupItemTypes_FatherId",
                table: "PointSetupItemTypes",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_PointSetupLines_FatherId",
                table: "PointSetupLines",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_PointSetupPackings_FatherId",
                table: "PointSetupPackings",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_PointSetupPackings_PackingId",
                table: "PointSetupPackings",
                column: "PackingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointSetupBrands");

            migrationBuilder.DropTable(
                name: "PointSetupCustomers");

            migrationBuilder.DropTable(
                name: "PointSetupIndustries");

            migrationBuilder.DropTable(
                name: "PointSetupItemTypes");

            migrationBuilder.DropTable(
                name: "PointSetupPackings");

            migrationBuilder.DropTable(
                name: "PointSetupLines");

            migrationBuilder.DropTable(
                name: "PointSetups");
        }
    }
}
