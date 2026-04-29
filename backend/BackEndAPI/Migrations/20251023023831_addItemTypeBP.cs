using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class addItemTypeBP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "m2m_BpClassifyItemType",
                columns: table => new
                {
                    BpClassifyId = table.Column<int>(type: "int", nullable: false),
                    ItemTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_m2m_BpClassifyItemType", x => new { x.BpClassifyId, x.ItemTypeId });
                    table.ForeignKey(
                        name: "FK_m2m_BpClassifyItemType_BpClassify_BpClassifyId",
                        column: x => x.BpClassifyId,
                        principalTable: "BpClassify",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_m2m_BpClassifyItemType_ItemType_ItemTypeId",
                        column: x => x.ItemTypeId,
                        principalTable: "ItemType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_m2m_BpClassifyItemType_ItemTypeId",
                table: "m2m_BpClassifyItemType",
                column: "ItemTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "m2m_BpClassifyItemType");
        }
    }
}
