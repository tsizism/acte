using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIPooc.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Username", "Email", "FirstName", "LastName", "CreatedAt", "UpdatedAt" },
                values: new object[] { 1, "admin", "admin@acte.com", "Admin", "User", DateTime.UtcNow, null }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1
            );
        }
    }
}
