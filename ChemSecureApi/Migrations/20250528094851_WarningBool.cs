using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChemSecureApi.Migrations
{
    /// <inheritdoc />
    public partial class WarningBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManaged",
                table: "Warnings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManagedDate",
                table: "Warnings",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManaged",
                table: "Warnings");

            migrationBuilder.DropColumn(
                name: "ManagedDate",
                table: "Warnings");
        }
    }
}
