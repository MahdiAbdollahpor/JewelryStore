using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelryStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class Attribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CategoryAttributes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CategoryAttributes");
        }
    }
}
