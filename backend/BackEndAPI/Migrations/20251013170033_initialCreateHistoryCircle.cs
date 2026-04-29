using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialCreateHistoryCircle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemCode",
                table: "CustomerPointHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "CustomerPointHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "CustomerPointHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemCode",
                table: "CustomerPointHistories");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "CustomerPointHistories");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "CustomerPointHistories");
        }
    }
}
