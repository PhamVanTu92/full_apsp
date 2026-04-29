using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialCreateZaloNoti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropUniqueConstraint(
            //    name: "AK_CRD4_InvoiceNumber",
            //    table: "CRD4");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_CRD4",
            //    table: "CRD4");

            //migrationBuilder.DropIndex(
            //    name: "IX_CRD4_InvoiceNumber",
            //    table: "CRD4");

            //migrationBuilder.AlterColumn<string>(
            //    name: "DocCode",
            //    table: "Payment",
            //    type: "nvarchar(450)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(50)");

            migrationBuilder.AddColumn<bool>(
                name: "ZaloCompleted",
                table: "ODOC",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ZaloConfirmed",
                table: "ODOC",
                type: "bit",
                nullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "InvoiceNumber",
            //    table: "CRD4",
            //    type: "nvarchar(450)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(50)",
            //    oldMaxLength: 50);

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_CRD4",
            //    table: "CRD4",
            //    column: "InvoiceNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_CRD4",
            //    table: "CRD4");

            migrationBuilder.DropColumn(
                name: "ZaloCompleted",
                table: "ODOC");

            migrationBuilder.DropColumn(
                name: "ZaloConfirmed",
                table: "ODOC");

            //migrationBuilder.AlterColumn<string>(
            //    name: "DocCode",
            //    table: "Payment",
            //    type: "nvarchar(50)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(450)");

            //migrationBuilder.AlterColumn<string>(
            //    name: "InvoiceNumber",
            //    table: "CRD4",
            //    type: "nvarchar(50)",
            //    maxLength: 50,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(450)");

            //migrationBuilder.AddUniqueConstraint(
            //    name: "AK_CRD4_InvoiceNumber",
            //    table: "CRD4",
            //    column: "InvoiceNumber");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_CRD4",
            //    table: "CRD4",
            //    column: "Id");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CRD4_InvoiceNumber",
            //    table: "CRD4",
            //    column: "InvoiceNumber",
            //    unique: true);
        }
    }
}
