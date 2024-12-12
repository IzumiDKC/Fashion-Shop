using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionShopDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalSpentAndRankToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Rank",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSpent",
                table: "AspNetUsers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rank",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TotalSpent",
                table: "AspNetUsers");
        }
    }
}
