using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialnew2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PackingId",
                table: "ExchangePointLine",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangePointLine_PackingId",
                table: "ExchangePointLine",
                column: "PackingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangePointLine_Packing_PackingId",
                table: "ExchangePointLine",
                column: "PackingId",
                principalTable: "Packing",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExchangePointLine_Packing_PackingId",
                table: "ExchangePointLine");

            migrationBuilder.DropIndex(
                name: "IX_ExchangePointLine_PackingId",
                table: "ExchangePointLine");

            migrationBuilder.DropColumn(
                name: "PackingId",
                table: "ExchangePointLine");
        }
    }
}
