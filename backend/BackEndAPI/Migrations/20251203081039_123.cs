using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class _123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalLevelLine_ApprovalLevel_FatherId",
                table: "ApprovalLevelLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalLevelLine_Users_ApprovalUserId",
                table: "ApprovalLevelLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalSampleDocumentsLine_ApprovalSample_FatherId",
                table: "ApprovalSampleDocumentsLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalSampleMembersLine_ApprovalSample_FatherId",
                table: "ApprovalSampleMembersLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalSampleMembersLine_Users_CreatorId",
                table: "ApprovalSampleMembersLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalSampleProcessesLine_ApprovalLevel_ApprovalLevelId",
                table: "ApprovalSampleProcessesLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalSampleProcessesLine_ApprovalSample_FatherId",
                table: "ApprovalSampleProcessesLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlow_ApprovalLevel_ApprovalLevelId",
                table: "ApprovalWorkFlow");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlow_ApprovalSample_ApprovalSampleId",
                table: "ApprovalWorkFlow");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlowDocumentLine_ApprovalWorkFlow_FatherId",
                table: "ApprovalWorkFlowDocumentLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlowLine_ApprovalLevel_ApprovalLevelId",
                table: "ApprovalWorkFlowLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlowLine_ApprovalWorkFlow_FatherId",
                table: "ApprovalWorkFlowLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlowLine_Users_ApprovalUserId",
                table: "ApprovalWorkFlowLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalWorkFlowLine",
                table: "ApprovalWorkFlowLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalWorkFlowDocumentLine",
                table: "ApprovalWorkFlowDocumentLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalWorkFlow",
                table: "ApprovalWorkFlow");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalSampleProcessesLine",
                table: "ApprovalSampleProcessesLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalSampleMembersLine",
                table: "ApprovalSampleMembersLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalSampleDocumentsLine",
                table: "ApprovalSampleDocumentsLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalSample",
                table: "ApprovalSample");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalLevelLine",
                table: "ApprovalLevelLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalLevel",
                table: "ApprovalLevel");

            migrationBuilder.RenameTable(
                name: "ApprovalWorkFlowLine",
                newName: "ApprovalWorkFlowLines");

            migrationBuilder.RenameTable(
                name: "ApprovalWorkFlowDocumentLine",
                newName: "ApprovalWorkFlowDocumentLines");

            migrationBuilder.RenameTable(
                name: "ApprovalWorkFlow",
                newName: "ApprovalWorkFlows");

            migrationBuilder.RenameTable(
                name: "ApprovalSampleProcessesLine",
                newName: "ApprovalSampleProcessesLines");

            migrationBuilder.RenameTable(
                name: "ApprovalSampleMembersLine",
                newName: "ApprovalSampleMembersLines");

            migrationBuilder.RenameTable(
                name: "ApprovalSampleDocumentsLine",
                newName: "ApprovalSampleDocumentsLines");

            migrationBuilder.RenameTable(
                name: "ApprovalSample",
                newName: "ApprovalSamples");

            migrationBuilder.RenameTable(
                name: "ApprovalLevelLine",
                newName: "ApprovalLevelLines");

            migrationBuilder.RenameTable(
                name: "ApprovalLevel",
                newName: "ApprovalLevels");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalWorkFlowLine_FatherId",
                table: "ApprovalWorkFlowLines",
                newName: "IX_ApprovalWorkFlowLines_FatherId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalWorkFlowLine_ApprovalUserId",
                table: "ApprovalWorkFlowLines",
                newName: "IX_ApprovalWorkFlowLines_ApprovalUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalWorkFlowLine_ApprovalLevelId",
                table: "ApprovalWorkFlowLines",
                newName: "IX_ApprovalWorkFlowLines_ApprovalLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalWorkFlowDocumentLine_FatherId",
                table: "ApprovalWorkFlowDocumentLines",
                newName: "IX_ApprovalWorkFlowDocumentLines_FatherId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalWorkFlow_ApprovalSampleId",
                table: "ApprovalWorkFlows",
                newName: "IX_ApprovalWorkFlows_ApprovalSampleId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalWorkFlow_ApprovalLevelId",
                table: "ApprovalWorkFlows",
                newName: "IX_ApprovalWorkFlows_ApprovalLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalSampleProcessesLine_FatherId",
                table: "ApprovalSampleProcessesLines",
                newName: "IX_ApprovalSampleProcessesLines_FatherId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalSampleProcessesLine_ApprovalLevelId",
                table: "ApprovalSampleProcessesLines",
                newName: "IX_ApprovalSampleProcessesLines_ApprovalLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalSampleMembersLine_FatherId",
                table: "ApprovalSampleMembersLines",
                newName: "IX_ApprovalSampleMembersLines_FatherId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalSampleMembersLine_CreatorId",
                table: "ApprovalSampleMembersLines",
                newName: "IX_ApprovalSampleMembersLines_CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalSampleDocumentsLine_FatherId",
                table: "ApprovalSampleDocumentsLines",
                newName: "IX_ApprovalSampleDocumentsLines_FatherId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalLevelLine_FatherId",
                table: "ApprovalLevelLines",
                newName: "IX_ApprovalLevelLines_FatherId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalLevelLine_ApprovalUserId",
                table: "ApprovalLevelLines",
                newName: "IX_ApprovalLevelLines_ApprovalUserId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ApprovalLevels",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalWorkFlowLines",
                table: "ApprovalWorkFlowLines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalWorkFlowDocumentLines",
                table: "ApprovalWorkFlowDocumentLines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalWorkFlows",
                table: "ApprovalWorkFlows",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalSampleProcessesLines",
                table: "ApprovalSampleProcessesLines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalSampleMembersLines",
                table: "ApprovalSampleMembersLines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalSampleDocumentsLines",
                table: "ApprovalSampleDocumentsLines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalSamples",
                table: "ApprovalSamples",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalLevelLines",
                table: "ApprovalLevelLines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalLevels",
                table: "ApprovalLevels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalLevelLines_ApprovalLevels_FatherId",
                table: "ApprovalLevelLines",
                column: "FatherId",
                principalTable: "ApprovalLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalLevelLines_Users_ApprovalUserId",
                table: "ApprovalLevelLines",
                column: "ApprovalUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalSampleDocumentsLines_ApprovalSamples_FatherId",
                table: "ApprovalSampleDocumentsLines",
                column: "FatherId",
                principalTable: "ApprovalSamples",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalSampleMembersLines_ApprovalSamples_FatherId",
                table: "ApprovalSampleMembersLines",
                column: "FatherId",
                principalTable: "ApprovalSamples",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalSampleMembersLines_Users_CreatorId",
                table: "ApprovalSampleMembersLines",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalSampleProcessesLines_ApprovalLevels_ApprovalLevelId",
                table: "ApprovalSampleProcessesLines",
                column: "ApprovalLevelId",
                principalTable: "ApprovalLevels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalSampleProcessesLines_ApprovalSamples_FatherId",
                table: "ApprovalSampleProcessesLines",
                column: "FatherId",
                principalTable: "ApprovalSamples",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlowDocumentLines_ApprovalWorkFlows_FatherId",
                table: "ApprovalWorkFlowDocumentLines",
                column: "FatherId",
                principalTable: "ApprovalWorkFlows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlowLines_ApprovalLevels_ApprovalLevelId",
                table: "ApprovalWorkFlowLines",
                column: "ApprovalLevelId",
                principalTable: "ApprovalLevels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlowLines_ApprovalWorkFlows_FatherId",
                table: "ApprovalWorkFlowLines",
                column: "FatherId",
                principalTable: "ApprovalWorkFlows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlowLines_Users_ApprovalUserId",
                table: "ApprovalWorkFlowLines",
                column: "ApprovalUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlows_ApprovalLevels_ApprovalLevelId",
                table: "ApprovalWorkFlows",
                column: "ApprovalLevelId",
                principalTable: "ApprovalLevels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlows_ApprovalSamples_ApprovalSampleId",
                table: "ApprovalWorkFlows",
                column: "ApprovalSampleId",
                principalTable: "ApprovalSamples",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalLevelLines_ApprovalLevels_FatherId",
                table: "ApprovalLevelLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalLevelLines_Users_ApprovalUserId",
                table: "ApprovalLevelLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalSampleDocumentsLines_ApprovalSamples_FatherId",
                table: "ApprovalSampleDocumentsLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalSampleMembersLines_ApprovalSamples_FatherId",
                table: "ApprovalSampleMembersLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalSampleMembersLines_Users_CreatorId",
                table: "ApprovalSampleMembersLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalSampleProcessesLines_ApprovalLevels_ApprovalLevelId",
                table: "ApprovalSampleProcessesLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalSampleProcessesLines_ApprovalSamples_FatherId",
                table: "ApprovalSampleProcessesLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlowDocumentLines_ApprovalWorkFlows_FatherId",
                table: "ApprovalWorkFlowDocumentLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlowLines_ApprovalLevels_ApprovalLevelId",
                table: "ApprovalWorkFlowLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlowLines_ApprovalWorkFlows_FatherId",
                table: "ApprovalWorkFlowLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlowLines_Users_ApprovalUserId",
                table: "ApprovalWorkFlowLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlows_ApprovalLevels_ApprovalLevelId",
                table: "ApprovalWorkFlows");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlows_ApprovalSamples_ApprovalSampleId",
                table: "ApprovalWorkFlows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalWorkFlows",
                table: "ApprovalWorkFlows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalWorkFlowLines",
                table: "ApprovalWorkFlowLines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalWorkFlowDocumentLines",
                table: "ApprovalWorkFlowDocumentLines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalSamples",
                table: "ApprovalSamples");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalSampleProcessesLines",
                table: "ApprovalSampleProcessesLines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalSampleMembersLines",
                table: "ApprovalSampleMembersLines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalSampleDocumentsLines",
                table: "ApprovalSampleDocumentsLines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalLevels",
                table: "ApprovalLevels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalLevelLines",
                table: "ApprovalLevelLines");

            migrationBuilder.RenameTable(
                name: "ApprovalWorkFlows",
                newName: "ApprovalWorkFlow");

            migrationBuilder.RenameTable(
                name: "ApprovalWorkFlowLines",
                newName: "ApprovalWorkFlowLine");

            migrationBuilder.RenameTable(
                name: "ApprovalWorkFlowDocumentLines",
                newName: "ApprovalWorkFlowDocumentLine");

            migrationBuilder.RenameTable(
                name: "ApprovalSamples",
                newName: "ApprovalSample");

            migrationBuilder.RenameTable(
                name: "ApprovalSampleProcessesLines",
                newName: "ApprovalSampleProcessesLine");

            migrationBuilder.RenameTable(
                name: "ApprovalSampleMembersLines",
                newName: "ApprovalSampleMembersLine");

            migrationBuilder.RenameTable(
                name: "ApprovalSampleDocumentsLines",
                newName: "ApprovalSampleDocumentsLine");

            migrationBuilder.RenameTable(
                name: "ApprovalLevels",
                newName: "ApprovalLevel");

            migrationBuilder.RenameTable(
                name: "ApprovalLevelLines",
                newName: "ApprovalLevelLine");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalWorkFlows_ApprovalSampleId",
                table: "ApprovalWorkFlow",
                newName: "IX_ApprovalWorkFlow_ApprovalSampleId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalWorkFlows_ApprovalLevelId",
                table: "ApprovalWorkFlow",
                newName: "IX_ApprovalWorkFlow_ApprovalLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalWorkFlowLines_FatherId",
                table: "ApprovalWorkFlowLine",
                newName: "IX_ApprovalWorkFlowLine_FatherId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalWorkFlowLines_ApprovalUserId",
                table: "ApprovalWorkFlowLine",
                newName: "IX_ApprovalWorkFlowLine_ApprovalUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalWorkFlowLines_ApprovalLevelId",
                table: "ApprovalWorkFlowLine",
                newName: "IX_ApprovalWorkFlowLine_ApprovalLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalWorkFlowDocumentLines_FatherId",
                table: "ApprovalWorkFlowDocumentLine",
                newName: "IX_ApprovalWorkFlowDocumentLine_FatherId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalSampleProcessesLines_FatherId",
                table: "ApprovalSampleProcessesLine",
                newName: "IX_ApprovalSampleProcessesLine_FatherId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalSampleProcessesLines_ApprovalLevelId",
                table: "ApprovalSampleProcessesLine",
                newName: "IX_ApprovalSampleProcessesLine_ApprovalLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalSampleMembersLines_FatherId",
                table: "ApprovalSampleMembersLine",
                newName: "IX_ApprovalSampleMembersLine_FatherId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalSampleMembersLines_CreatorId",
                table: "ApprovalSampleMembersLine",
                newName: "IX_ApprovalSampleMembersLine_CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalSampleDocumentsLines_FatherId",
                table: "ApprovalSampleDocumentsLine",
                newName: "IX_ApprovalSampleDocumentsLine_FatherId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalLevelLines_FatherId",
                table: "ApprovalLevelLine",
                newName: "IX_ApprovalLevelLine_FatherId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalLevelLines_ApprovalUserId",
                table: "ApprovalLevelLine",
                newName: "IX_ApprovalLevelLine_ApprovalUserId");

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                table: "ApprovalLevel",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalWorkFlow",
                table: "ApprovalWorkFlow",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalWorkFlowLine",
                table: "ApprovalWorkFlowLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalWorkFlowDocumentLine",
                table: "ApprovalWorkFlowDocumentLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalSample",
                table: "ApprovalSample",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalSampleProcessesLine",
                table: "ApprovalSampleProcessesLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalSampleMembersLine",
                table: "ApprovalSampleMembersLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalSampleDocumentsLine",
                table: "ApprovalSampleDocumentsLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalLevel",
                table: "ApprovalLevel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalLevelLine",
                table: "ApprovalLevelLine",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalLevelLine_ApprovalLevel_FatherId",
                table: "ApprovalLevelLine",
                column: "FatherId",
                principalTable: "ApprovalLevel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalLevelLine_Users_ApprovalUserId",
                table: "ApprovalLevelLine",
                column: "ApprovalUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalSampleDocumentsLine_ApprovalSample_FatherId",
                table: "ApprovalSampleDocumentsLine",
                column: "FatherId",
                principalTable: "ApprovalSample",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalSampleMembersLine_ApprovalSample_FatherId",
                table: "ApprovalSampleMembersLine",
                column: "FatherId",
                principalTable: "ApprovalSample",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalSampleMembersLine_Users_CreatorId",
                table: "ApprovalSampleMembersLine",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalSampleProcessesLine_ApprovalLevel_ApprovalLevelId",
                table: "ApprovalSampleProcessesLine",
                column: "ApprovalLevelId",
                principalTable: "ApprovalLevel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalSampleProcessesLine_ApprovalSample_FatherId",
                table: "ApprovalSampleProcessesLine",
                column: "FatherId",
                principalTable: "ApprovalSample",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlow_ApprovalLevel_ApprovalLevelId",
                table: "ApprovalWorkFlow",
                column: "ApprovalLevelId",
                principalTable: "ApprovalLevel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlow_ApprovalSample_ApprovalSampleId",
                table: "ApprovalWorkFlow",
                column: "ApprovalSampleId",
                principalTable: "ApprovalSample",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlowDocumentLine_ApprovalWorkFlow_FatherId",
                table: "ApprovalWorkFlowDocumentLine",
                column: "FatherId",
                principalTable: "ApprovalWorkFlow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlowLine_ApprovalLevel_ApprovalLevelId",
                table: "ApprovalWorkFlowLine",
                column: "ApprovalLevelId",
                principalTable: "ApprovalLevel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlowLine_ApprovalWorkFlow_FatherId",
                table: "ApprovalWorkFlowLine",
                column: "FatherId",
                principalTable: "ApprovalWorkFlow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlowLine_Users_ApprovalUserId",
                table: "ApprovalWorkFlowLine",
                column: "ApprovalUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
