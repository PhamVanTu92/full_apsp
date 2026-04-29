using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class _1157 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<string>(
            //     name: "ReasonForCancellation",
            //     table: "ODOC",
            //     type: "nvarchar(1000)",
            //     maxLength: 1000,
            //     nullable: true);
            //
            // migrationBuilder.AddColumn<string>(
            //     name: "AreaName",
            //     table: "DOC12",
            //     type: "nvarchar(254)",
            //     maxLength: 254,
            //     nullable: true);
            //
            // migrationBuilder.AddColumn<string>(
            //     name: "CCCD",
            //     table: "DOC12",
            //     type: "nvarchar(50)",
            //     maxLength: 50,
            //     nullable: true);
            //
            // migrationBuilder.AddColumn<string>(
            //     name: "Email",
            //     table: "DOC12",
            //     type: "nvarchar(50)",
            //     maxLength: 50,
            //     nullable: true);
            //
            // migrationBuilder.AddColumn<string>(
            //     name: "Person",
            //     table: "DOC12",
            //     type: "nvarchar(100)",
            //     maxLength: 100,
            //     nullable: true);
            //
            // migrationBuilder.AddColumn<string>(
            //     name: "Phone",
            //     table: "DOC12",
            //     type: "nvarchar(50)",
            //     maxLength: 50,
            //     nullable: true);
            //
            // migrationBuilder.AddColumn<string>(
            //     name: "Type",
            //     table: "DOC12",
            //     type: "nvarchar(25)",
            //     maxLength: 25,
            //     nullable: true);
            //
            // migrationBuilder.AddColumn<string>(
            //     name: "VehiclePlate",
            //     table: "DOC12",
            //     type: "nvarchar(50)",
            //     maxLength: 50,
            //     nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReasonForCancellation",
                table: "ODOC");

            migrationBuilder.DropColumn(
                name: "AreaName",
                table: "DOC12");

            migrationBuilder.DropColumn(
                name: "CCCD",
                table: "DOC12");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "DOC12");

            migrationBuilder.DropColumn(
                name: "Person",
                table: "DOC12");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "DOC12");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "DOC12");

            migrationBuilder.DropColumn(
                name: "VehiclePlate",
                table: "DOC12");
        }
    }
}
