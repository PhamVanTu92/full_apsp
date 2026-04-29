using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class intitalupdateOpenQty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddCheckConstraint(
            //    name: "CK_ItemDetail_OpenQty",
            //    table: "DOC1",
            //    sql: "[OpenQty] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropCheckConstraint(
            //    name: "CK_ItemDetail_OpenQty",
            //    table: "DOC1");
        }
    }
}
