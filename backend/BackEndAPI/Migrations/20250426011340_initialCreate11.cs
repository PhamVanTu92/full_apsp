using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommittedLineSubId",
                table: "CommittedTrackingLine",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CommittedTrackingLine_CommittedLineSubId",
                table: "CommittedTrackingLine",
                column: "CommittedLineSubId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommittedTrackingLine_CommittedLineSub_CommittedLineSubId",
                table: "CommittedTrackingLine",
                column: "CommittedLineSubId",
                principalTable: "CommittedLineSub",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommittedTrackingLine_CommittedLineSub_CommittedLineSubId",
                table: "CommittedTrackingLine");

            migrationBuilder.DropIndex(
                name: "IX_CommittedTrackingLine_CommittedLineSubId",
                table: "CommittedTrackingLine");

            migrationBuilder.DropColumn(
                name: "CommittedLineSubId",
                table: "CommittedTrackingLine");
        }
    }
}
