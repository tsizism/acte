using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class AddedTransactionTypeWatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add HoldingId column, index, and FK
            migrationBuilder.AddColumn<int>(
                name: "HoldingId",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_HoldingId",
                table: "Transactions",
                column: "HoldingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Holdings_HoldingId",
                table: "Transactions",
                column: "HoldingId",
                principalTable: "Holdings",
                principalColumn: "HoldingId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Holdings_HoldingId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_HoldingId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "HoldingId",
                table: "Transactions");
        }
    }
}
