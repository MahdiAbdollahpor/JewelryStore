using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelryStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class mag2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VerificationCode",
                schema: "dbo",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationCodeExpiry",
                schema: "dbo",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerificationCode",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VerificationCodeExpiry",
                schema: "dbo",
                table: "Users");
        }
    }
}
