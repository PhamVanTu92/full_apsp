using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class new_approval_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalLevel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalLevelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalNumber = table.Column<int>(type: "int", nullable: false),
                    DeclineNumber = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalLevel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalSample",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalSampleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDebtLimit = table.Column<bool>(type: "bit", nullable: false),
                    IsOverdueDebt = table.Column<bool>(type: "bit", nullable: false),
                    IsOther = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalSample", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalLevelLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    ApprovalUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalLevelLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalLevelLine_ApprovalLevel_FatherId",
                        column: x => x.FatherId,
                        principalTable: "ApprovalLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalLevelLine_Users_ApprovalUserId",
                        column: x => x.ApprovalUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApprovalSampleDocumentsLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    Document = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalSampleDocumentsLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalSampleDocumentsLine_ApprovalSample_FatherId",
                        column: x => x.FatherId,
                        principalTable: "ApprovalSample",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalSampleMembersLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalSampleMembersLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalSampleMembersLine_ApprovalSample_FatherId",
                        column: x => x.FatherId,
                        principalTable: "ApprovalSample",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalSampleMembersLine_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApprovalSampleProcessesLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<int>(type: "int", nullable: false),
                    ApprovalLevelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalSampleProcessesLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalSampleProcessesLine_ApprovalLevel_ApprovalLevelId",
                        column: x => x.ApprovalLevelId,
                        principalTable: "ApprovalLevel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalSampleProcessesLine_ApprovalSample_FatherId",
                        column: x => x.FatherId,
                        principalTable: "ApprovalSample",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLevelLine_ApprovalUserId",
                table: "ApprovalLevelLine",
                column: "ApprovalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLevelLine_FatherId",
                table: "ApprovalLevelLine",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSampleDocumentsLine_FatherId",
                table: "ApprovalSampleDocumentsLine",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSampleMembersLine_CreatorId",
                table: "ApprovalSampleMembersLine",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSampleMembersLine_FatherId",
                table: "ApprovalSampleMembersLine",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSampleProcessesLine_ApprovalLevelId",
                table: "ApprovalSampleProcessesLine",
                column: "ApprovalLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSampleProcessesLine_FatherId",
                table: "ApprovalSampleProcessesLine",
                column: "FatherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalLevelLine");

            migrationBuilder.DropTable(
                name: "ApprovalSampleDocumentsLine");

            migrationBuilder.DropTable(
                name: "ApprovalSampleMembersLine");

            migrationBuilder.DropTable(
                name: "ApprovalSampleProcessesLine");

            migrationBuilder.DropTable(
                name: "ApprovalLevel");

            migrationBuilder.DropTable(
                name: "ApprovalSample");
        }
    }
}
