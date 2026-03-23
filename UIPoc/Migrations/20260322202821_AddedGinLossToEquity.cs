using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class AddedGinLossToEquity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Equities");

            migrationBuilder.AddColumn<decimal>(
                name: "GainLoss",
                table: "Equities",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GainLoss",
                table: "Equities");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Equities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
