using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class SymbolIsUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EquityMarkets_Symbol",
                table: "EquityMarkets");

            migrationBuilder.CreateIndex(
                name: "IX_EquityMarkets_Symbol",
                table: "EquityMarkets",
                column: "Symbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equities_Symbol",
                table: "Equities",
                column: "Symbol",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EquityMarkets_Symbol",
                table: "EquityMarkets");

            migrationBuilder.DropIndex(
                name: "IX_Equities_Symbol",
                table: "Equities");

            migrationBuilder.CreateIndex(
                name: "IX_EquityMarkets_Symbol",
                table: "EquityMarkets",
                column: "Symbol");
        }
    }
}
