using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class IndexHistoryDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IndexHistories_HoldingId_RecordedAt",
                table: "IndexHistories");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "RecordedAt",
                table: "IndexHistories",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_IndexHistories_HoldingId_RecordedAt",
                table: "IndexHistories",
                columns: new[] { "HoldingId", "RecordedAt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IndexHistories_HoldingId_RecordedAt",
                table: "IndexHistories");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordedAt",
                table: "IndexHistories",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.CreateIndex(
                name: "IX_IndexHistories_HoldingId_RecordedAt",
                table: "IndexHistories",
                columns: new[] { "HoldingId", "RecordedAt" });
        }
    }
}
