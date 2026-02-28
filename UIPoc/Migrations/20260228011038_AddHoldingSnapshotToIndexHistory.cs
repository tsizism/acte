using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class AddHoldingSnapshotToIndexHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HoldingSnapshot",
                table: "IndexHistories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoldingSnapshot",
                table: "IndexHistories");
        }
    }
}
