using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class AddedHoldingFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Flag",
                table: "Holdings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FlagDate",
                table: "Holdings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagMessage",
                table: "Holdings",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FlagDate",
                table: "Equities",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flag",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "FlagDate",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "FlagMessage",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "FlagDate",
                table: "Equities");
        }
    }
}
