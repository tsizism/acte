using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class RefactorHoldingsToEquities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Holdings_UserId_Symbol",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "AverageCost",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "CurrentPrice",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "Holdings");

            migrationBuilder.AddColumn<double>(
                name: "Index",
                table: "Holdings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "Equities",
                columns: table => new
                {
                    EquityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoldingId = table.Column<int>(type: "int", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AverageCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equities", x => x.EquityId);
                    table.ForeignKey(
                        name: "FK_Equities_Holdings_HoldingId",
                        column: x => x.HoldingId,
                        principalTable: "Holdings",
                        principalColumn: "HoldingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Holdings_UserId",
                table: "Holdings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Equities_HoldingId",
                table: "Equities",
                column: "HoldingId");

            migrationBuilder.CreateIndex(
                name: "IX_Equities_HoldingId_Symbol",
                table: "Equities",
                columns: new[] { "HoldingId", "Symbol" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Equities");

            migrationBuilder.DropIndex(
                name: "IX_Holdings_UserId",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "Holdings");

            migrationBuilder.AddColumn<decimal>(
                name: "AverageCost",
                table: "Holdings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Holdings",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentPrice",
                table: "Holdings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "Holdings",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "Holdings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Holdings_UserId_Symbol",
                table: "Holdings",
                columns: new[] { "UserId", "Symbol" });
        }
    }
}
