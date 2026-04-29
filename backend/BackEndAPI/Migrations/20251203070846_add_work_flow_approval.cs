using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class add_work_flow_approval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalWorkFlow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocId = table.Column<int>(type: "int", nullable: false),
                    ApprovalSampleId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    ApprovalNumber = table.Column<int>(type: "int", nullable: false),
                    DeclineNumber = table.Column<int>(type: "int", nullable: false),
                    ApprovalLevelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalWorkFlow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalWorkFlow_ApprovalLevel_ApprovalLevelId",
                        column: x => x.ApprovalLevelId,
                        principalTable: "ApprovalLevel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalWorkFlow_ApprovalSample_ApprovalSampleId",
                        column: x => x.ApprovalSampleId,
                        principalTable: "ApprovalSample",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApprovalWorkFlowDocumentLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    DocId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalWorkFlowDocumentLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalWorkFlowDocumentLine_ApprovalWorkFlow_FatherId",
                        column: x => x.FatherId,
                        principalTable: "ApprovalWorkFlow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalWorkFlowLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    ApprovalUserId = table.Column<int>(type: "int", nullable: false),
                    ApprovalLevelId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SortId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalWorkFlowLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalWorkFlowLine_ApprovalLevel_ApprovalLevelId",
                        column: x => x.ApprovalLevelId,
                        principalTable: "ApprovalLevel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalWorkFlowLine_ApprovalWorkFlow_FatherId",
                        column: x => x.FatherId,
                        principalTable: "ApprovalWorkFlow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalWorkFlowLine_Users_ApprovalUserId",
                        column: x => x.ApprovalUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkFlow_ApprovalLevelId",
                table: "ApprovalWorkFlow",
                column: "ApprovalLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkFlow_ApprovalSampleId",
                table: "ApprovalWorkFlow",
                column: "ApprovalSampleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkFlowDocumentLine_FatherId",
                table: "ApprovalWorkFlowDocumentLine",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkFlowLine_ApprovalLevelId",
                table: "ApprovalWorkFlowLine",
                column: "ApprovalLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkFlowLine_ApprovalUserId",
                table: "ApprovalWorkFlowLine",
                column: "ApprovalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkFlowLine_FatherId",
                table: "ApprovalWorkFlowLine",
                column: "FatherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalWorkFlowDocumentLine");

            migrationBuilder.DropTable(
                name: "ApprovalWorkFlowLine");

            migrationBuilder.DropTable(
                name: "ApprovalWorkFlow");
        }
    }
}
