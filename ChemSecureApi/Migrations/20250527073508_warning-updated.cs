using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChemSecureApi.Migrations
{
    /// <inheritdoc />
    public partial class warningupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TankId",
                table: "Warnings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Warnings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TankId",
                table: "Warnings");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Warnings");
        }
    }
}
