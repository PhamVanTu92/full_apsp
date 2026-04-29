using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class _102234213 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropIndex(
            //     name: "IX_Approval_DocId",
            //     table: "Approval");
            //
            // migrationBuilder.CreateIndex(
            //     name: "IX_Approval_DocId",
            //     table: "Approval",
            //     column: "DocId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Approval_DocId",
                table: "Approval");

            migrationBuilder.CreateIndex(
                name: "IX_Approval_DocId",
                table: "Approval",
                column: "DocId",
                unique: true);
        }
    }
}
