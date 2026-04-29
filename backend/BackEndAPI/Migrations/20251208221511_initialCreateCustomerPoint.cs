using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialCreateCustomerPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerPoint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    DocId = table.Column<int>(type: "int", nullable: false),
                    DocType = table.Column<int>(type: "int", nullable: false),
                    AddPoint = table.Column<bool>(type: "bit", nullable: false),
                    TotalPointChange = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerPoint", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerPointLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    DocId = table.Column<int>(type: "int", nullable: true),
                    DocType = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    ItemCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PoitnSetupId = table.Column<int>(type: "int", nullable: false),
                    PointChange = table.Column<double>(type: "float", nullable: false),
                    PointBefore = table.Column<double>(type: "float", nullable: false),
                    PointAfter = table.Column<double>(type: "float", nullable: false),
                    DocDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerPointLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerPointLine_CustomerPoint_FatherId",
                        column: x => x.FatherId,
                        principalTable: "CustomerPoint",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPoint_DocId_DocType_AddPoint",
                table: "CustomerPoint",
                columns: new[] { "DocId", "DocType", "AddPoint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPointLine_FatherId",
                table: "CustomerPointLine",
                column: "FatherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerPointLine");

            migrationBuilder.DropTable(
                name: "CustomerPoint");
        }
    }
}
