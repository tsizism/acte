using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using UIPooc.Data;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(HoldingsDbContext))]
    [Migration("20261101000000_RenameLastTxnPriceToLastTxnMarketPrice")]
    public partial class RenameLastTxnPriceToLastTxnMarketPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastTxnPrice",
                table: "Equities",
                newName: "LastTxnMarketPrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastTxnMarketPrice",
                table: "Equities",
                newName: "LastTxnPrice");
        }
    }
}
