using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class _0500 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleFillCustomerGroup",
                columns: table => new
                {
                    CustomerGroupId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleFillCustomerGroup", x => new { x.CustomerGroupId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_RoleFillCustomerGroup_OCRG_CustomerGroupId",
                        column: x => x.CustomerGroupId,
                        principalTable: "OCRG",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoleFillCustomerGroup_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleFillCustomerGroup_RoleId",
                table: "RoleFillCustomerGroup",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleFillCustomerGroup");
        }
    }
}
