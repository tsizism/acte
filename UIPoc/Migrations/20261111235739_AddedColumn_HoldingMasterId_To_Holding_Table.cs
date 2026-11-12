using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class AddedColumn_HoldingMasterId_To_Holding_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HoldingMasterId",
                table: "Holdings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Holdings_HoldingMasterId",
                table: "Holdings",
                column: "HoldingMasterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Holdings_HoldingMasterId",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "HoldingMasterId",
                table: "Holdings");
        }
    }
}
