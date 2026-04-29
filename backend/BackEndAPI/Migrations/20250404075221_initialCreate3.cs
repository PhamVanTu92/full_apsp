using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestSampleLines");

            migrationBuilder.DropTable(
                name: "TestSamples");

            migrationBuilder.AddColumn<bool>(
                name: "AddAccumulate",
                table: "DOC2",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasException",
                table: "DOC2",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOtherDist",
                table: "DOC2",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOtherDistExc",
                table: "DOC2",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOtherPay",
                table: "DOC2",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOtherPayExc",
                table: "DOC2",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOtherPromotion",
                table: "DOC2",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOtherPromotionExc",
                table: "DOC2",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddAccumulate",
                table: "DOC2");

            migrationBuilder.DropColumn(
                name: "HasException",
                table: "DOC2");

            migrationBuilder.DropColumn(
                name: "IsOtherDist",
                table: "DOC2");

            migrationBuilder.DropColumn(
                name: "IsOtherDistExc",
                table: "DOC2");

            migrationBuilder.DropColumn(
                name: "IsOtherPay",
                table: "DOC2");

            migrationBuilder.DropColumn(
                name: "IsOtherPayExc",
                table: "DOC2");

            migrationBuilder.DropColumn(
                name: "IsOtherPromotion",
                table: "DOC2");

            migrationBuilder.DropColumn(
                name: "IsOtherPromotionExc",
                table: "DOC2");

            migrationBuilder.CreateTable(
                name: "TestSamples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BpId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CustomerType = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    IsNewCustomer = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSamples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestSamples_OCRD_BpId",
                        column: x => x.BpId,
                        principalTable: "OCRD",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestSamples_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestSampleLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    TestResult = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    TestSampleId = table.Column<int>(type: "int", nullable: false),
                    Volume = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSampleLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestSampleLines_OITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "OITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestSampleLines_TestSamples_TestSampleId",
                        column: x => x.TestSampleId,
                        principalTable: "TestSamples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestSampleLines_ItemId",
                table: "TestSampleLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSampleLines_TestSampleId",
                table: "TestSampleLines",
                column: "TestSampleId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSamples_BpId",
                table: "TestSamples",
                column: "BpId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSamples_UserId",
                table: "TestSamples",
                column: "UserId");
        }
    }
}
