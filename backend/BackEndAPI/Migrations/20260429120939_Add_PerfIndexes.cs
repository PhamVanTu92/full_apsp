using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class Add_PerfIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WTM2_Sort",
                table: "WTM2",
                column: "Sort");

            migrationBuilder.CreateIndex(
                name: "IX_Promotion_Status_FromDate_ToDate",
                table: "Promotion",
                columns: new[] { "PromotionStatus", "FromDate", "ToDate" });

            migrationBuilder.CreateIndex(
                name: "IX_OWTM_Active",
                table: "OWTM",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSpec_Industry_Brand_ItemType",
                table: "ItemSpec",
                columns: new[] { "IndustryId", "BrandId", "ItemTypeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WTM2_Sort",
                table: "WTM2");

            migrationBuilder.DropIndex(
                name: "IX_Promotion_Status_FromDate_ToDate",
                table: "Promotion");

            migrationBuilder.DropIndex(
                name: "IX_OWTM_Active",
                table: "OWTM");

            migrationBuilder.DropIndex(
                name: "IX_ItemSpec_Industry_Brand_ItemType",
                table: "ItemSpec");
        }
    }
}
