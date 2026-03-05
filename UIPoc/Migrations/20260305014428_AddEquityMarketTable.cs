using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class AddEquityMarketTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EquityMarkets",
                columns: table => new
                {
                    EquityMarketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Market = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousClose = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpenPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DayHigh = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DayLow = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    MarketCap = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Week52High = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Week52Low = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PERatio = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    DividendYield = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    EPS = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastTradeTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Exchange = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquityMarkets", x => x.EquityMarketId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquityMarkets_LastUpdated",
                table: "EquityMarkets",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_EquityMarkets_Market",
                table: "EquityMarkets",
                column: "Market");

            migrationBuilder.CreateIndex(
                name: "IX_EquityMarkets_Symbol",
                table: "EquityMarkets",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_EquityMarkets_Symbol_Market",
                table: "EquityMarkets",
                columns: new[] { "Symbol", "Market" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquityMarkets");
        }
    }
}
