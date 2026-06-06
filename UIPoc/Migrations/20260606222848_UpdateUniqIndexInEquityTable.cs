using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUniqIndexInEquityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EquityMarkets_LastUpdated",
                table: "EquityMarkets");

            migrationBuilder.DropIndex(
                name: "IX_Equities_HoldingId_Symbol",
                table: "Equities");

            migrationBuilder.DropIndex(
                name: "IX_Equities_Symbol",
                table: "Equities");

            migrationBuilder.CreateIndex(
                name: "IX_Equities_HoldingId_Symbol",
                table: "Equities",
                columns: new[] { "HoldingId", "Symbol" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Equities_HoldingId_Symbol",
                table: "Equities");

            migrationBuilder.CreateIndex(
                name: "IX_EquityMarkets_LastUpdated",
                table: "EquityMarkets",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_Equities_HoldingId_Symbol",
                table: "Equities",
                columns: new[] { "HoldingId", "Symbol" });

            migrationBuilder.CreateIndex(
                name: "IX_Equities_Symbol",
                table: "Equities",
                column: "Symbol",
                unique: true);
        }
    }
}
