using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class RemoveColumnsFromEquityMarket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EquityMarkets_Market",
                table: "EquityMarkets");

            migrationBuilder.DropIndex(
                name: "IX_EquityMarkets_Symbol_Market",
                table: "EquityMarkets");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "EquityMarkets");

            migrationBuilder.DropColumn(
                name: "DividendYield",
                table: "EquityMarkets");

            migrationBuilder.DropColumn(
                name: "EPS",
                table: "EquityMarkets");

            migrationBuilder.DropColumn(
                name: "Exchange",
                table: "EquityMarkets");

            migrationBuilder.DropColumn(
                name: "Market",
                table: "EquityMarkets");

            migrationBuilder.DropColumn(
                name: "PERatio",
                table: "EquityMarkets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "EquityMarkets",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DividendYield",
                table: "EquityMarkets",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EPS",
                table: "EquityMarkets",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Exchange",
                table: "EquityMarkets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Market",
                table: "EquityMarkets",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PERatio",
                table: "EquityMarkets",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquityMarkets_Market",
                table: "EquityMarkets",
                column: "Market");

            migrationBuilder.CreateIndex(
                name: "IX_EquityMarkets_Symbol_Market",
                table: "EquityMarkets",
                columns: new[] { "Symbol", "Market" },
                unique: true);
        }
    }
}
