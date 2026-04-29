using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "CommittedTrackingLine");

            //migrationBuilder.DropTable(
            //    name: "CommittedTracking");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "CommittedTracking",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CommittedLineSubId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CommittedTracking", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CommittedTrackingLine",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        BonusApplied = table.Column<double>(type: "float", nullable: false),
            //        CommittedType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DocId = table.Column<int>(type: "int", nullable: false),
            //        FatherId = table.Column<int>(type: "int", nullable: false),
            //        OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Volume = table.Column<double>(type: "float", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CommittedTrackingLine", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_CommittedTrackingLine_CommittedTracking_FatherId",
            //            column: x => x.FatherId,
            //            principalTable: "CommittedTracking",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_CommittedTrackingLine_FatherId",
            //    table: "CommittedTrackingLine",
            //    column: "FatherId");
        }
    }
}
