using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class addgroupcustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalSampleOcrgLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    BusinessPartnerGpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalSampleOcrgLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalSampleOcrgLine_ApprovalSamples_FatherId",
                        column: x => x.FatherId,
                        principalTable: "ApprovalSamples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalSampleOcrgLine_OCRG_BusinessPartnerGpId",
                        column: x => x.BusinessPartnerGpId,
                        principalTable: "OCRG",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSampleOcrgLine_BusinessPartnerGpId",
                table: "ApprovalSampleOcrgLine",
                column: "BusinessPartnerGpId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSampleOcrgLine_FatherId",
                table: "ApprovalSampleOcrgLine",
                column: "FatherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalSampleOcrgLine");
        }
    }
}
