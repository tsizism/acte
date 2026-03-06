using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class AddHoldingType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Holdings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Holdings");
        }
    }
}
