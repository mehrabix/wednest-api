using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WedNest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "Code", "CreatedAt", "DisplayOrder", "IsActive", "IsDefault", "Name", "NativeName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "en", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, true, "English", "English", null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "ar", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, false, "Arabic", "العربية", null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "fa", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, false, "Persian", "فارسی", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));
        }
    }
}
