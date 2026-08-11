using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelryStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class mig3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Active_Stock_Featured",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsInStock",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsInStock",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInStock",
                table: "Products",
                type: "bit",
                nullable: false,
                computedColumnSql: "CASE WHEN Quantity > 0 THEN 1 ELSE 0 END",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Active_Stock_Featured",
                table: "Products",
                columns: new[] { "IsActive", "IsInStock", "IsFeatured" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsInStock",
                table: "Products",
                column: "IsInStock");
        }
    }
}
