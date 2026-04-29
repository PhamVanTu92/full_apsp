using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommittedLineSubId",
                table: "CommittedTracking");

            migrationBuilder.AddColumn<string>(
                name: "ListLineId",
                table: "DOC2",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCommitted",
                table: "CommittedTrackingLine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CommittedId",
                table: "CommittedTracking",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListLineId",
                table: "DOC2");

            migrationBuilder.DropColumn(
                name: "IsCommitted",
                table: "CommittedTrackingLine");

            migrationBuilder.DropColumn(
                name: "CommittedId",
                table: "CommittedTracking");

            migrationBuilder.AddColumn<int>(
                name: "CommittedLineSubId",
                table: "CommittedTracking",
                type: "int",
                nullable: true);
        }
    }
}
