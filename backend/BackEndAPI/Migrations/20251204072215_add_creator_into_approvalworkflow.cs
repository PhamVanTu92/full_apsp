using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class add_creator_into_approvalworkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "ApprovalWorkFlows",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkFlows_CreatorId",
                table: "ApprovalWorkFlows",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkFlows_Users_CreatorId",
                table: "ApprovalWorkFlows",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalWorkFlows_Users_CreatorId",
                table: "ApprovalWorkFlows");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalWorkFlows_CreatorId",
                table: "ApprovalWorkFlows");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "ApprovalWorkFlows");
        }
    }
}
