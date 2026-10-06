using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SM.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "AQAAAAIAAYagAAAAEClKbWHhNAcQjaJijzxVSOd1/9lnh4abMxw7Kzupf5A1GS8Hx59dRq6yVCSGnNEAmA==", "Yonetici", "yonetici" },
                    { 2, "AQAAAAIAAYagAAAAEJACifbWB7xxH9Hw7eT+VtQre3eA/Y97S3a/BjRPdCSIXAsJ0OvtZlM+TtW3MHPDIA==", "DepoSorumlusu", "depo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
