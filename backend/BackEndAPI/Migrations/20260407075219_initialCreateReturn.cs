using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialCreateReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BaseObj",
                table: "DOC1",
                newName: "BaseLineId");

            migrationBuilder.AddColumn<string>(
                name: "RefInvoiceCode",
                table: "ODOC",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OpenQty",
                table: "DOC1",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefInvoiceCode",
                table: "ODOC");

            migrationBuilder.DropColumn(
                name: "OpenQty",
                table: "DOC1");

            migrationBuilder.RenameColumn(
                name: "BaseLineId",
                table: "DOC1",
                newName: "BaseObj");
        }
    }
}
