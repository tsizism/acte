using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class AddedColumnsToEquityAndHoldin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CallName",
                table: "Holdings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Holdings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Holdings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "HoldingHigh",
                table: "Equities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HoldingHighAt",
                table: "Equities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "HoldingLow",
                table: "Equities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HoldingLowAt",
                table: "Equities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallName",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "HoldingHigh",
                table: "Equities");

            migrationBuilder.DropColumn(
                name: "HoldingHighAt",
                table: "Equities");

            migrationBuilder.DropColumn(
                name: "HoldingLow",
                table: "Equities");

            migrationBuilder.DropColumn(
                name: "HoldingLowAt",
                table: "Equities");
        }
    }
}
