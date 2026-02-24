using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class CreateIndexHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndexHistories",
                columns: table => new
                {
                    IndexHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoldingId = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<double>(type: "float", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexHistories", x => x.IndexHistoryId);
                    table.ForeignKey(
                        name: "FK_IndexHistories_Holdings_HoldingId",
                        column: x => x.HoldingId,
                        principalTable: "Holdings",
                        principalColumn: "HoldingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndexHistories_HoldingId",
                table: "IndexHistories",
                column: "HoldingId");

            migrationBuilder.CreateIndex(
                name: "IX_IndexHistories_HoldingId_RecordedAt",
                table: "IndexHistories",
                columns: new[] { "HoldingId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_IndexHistories_RecordedAt",
                table: "IndexHistories",
                column: "RecordedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndexHistories");
        }
    }
}
