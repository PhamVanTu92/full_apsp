using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialCreateZaloUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "templateCompleted",
                table: "ZaloAccess",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "templateConfirmed",
                table: "ZaloAccess",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ZaloError1",
                table: "ODOC",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "templateCompleted",
                table: "ZaloAccess");

            migrationBuilder.DropColumn(
                name: "templateConfirmed",
                table: "ZaloAccess");

            migrationBuilder.DropColumn(
                name: "ZaloError1",
                table: "ODOC");
        }
    }
}
