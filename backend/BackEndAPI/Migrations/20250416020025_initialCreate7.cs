using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAPI.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //name: "PaymentRule",
            //columns: table => new
            //{
            //    Id = table.Column<int>(nullable: false)
            //        .Annotation("SqlServer:Identity", "1, 1"),
            //    Name = table.Column<string>(nullable: true),
            //    PromotionTax = table.Column<decimal>(nullable: true),
            //    BonusPaynow = table.Column<decimal>(nullable: true),
            //    BonusVolumn = table.Column<decimal>(nullable: true),
            //},
            //constraints: table =>
            //{
            //    table.PrimaryKey("PK_Products", x => x.Id);
            //});
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
