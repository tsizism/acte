using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class AddLastTxnToEquity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LastTxnPrice",
                table: "Equities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastTxnQuantity",
                table: "Equities",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastTxnType",
                table: "Equities",
                type: "int",
                nullable: true);


            migrationBuilder.AddColumn<DateTime>(
                name: "LastTxnAt",
                table: "Equities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastTxnPrice",
                table: "Equities");

            migrationBuilder.DropColumn(
                name: "LastTxnQuantity",
                table: "Equities");

            migrationBuilder.DropColumn(
                name: "LastTxnAt",
                table: "Equities");

            migrationBuilder.DropColumn(
                name: "LastTxnType",
                table: "Equities");

        }
    }
}
