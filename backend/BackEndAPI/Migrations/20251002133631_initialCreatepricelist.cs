using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialCreatepricelist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriceLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PriceListName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAllCustomer = table.Column<bool>(type: "bit", nullable: false),
                    IsRetail = table.Column<bool>(type: "bit", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    CustomerGroupId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpriedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceLists_OCRD_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "OCRD",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PriceLists_OCRG_CustomerGroupId",
                        column: x => x.CustomerGroupId,
                        principalTable: "OCRG",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PriceListLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    PackingName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<double>(type: "float", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceListLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceListLines_PriceLists_FatherId",
                        column: x => x.FatherId,
                        principalTable: "PriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceListLines_FatherId",
                table: "PriceListLines",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_CustomerGroupId",
                table: "PriceLists",
                column: "CustomerGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_CustomerId",
                table: "PriceLists",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceListLines");

            migrationBuilder.DropTable(
                name: "PriceLists");
        }
    }
}
