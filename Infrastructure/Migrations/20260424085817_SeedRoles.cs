using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "31cb0cfb-5810-48df-b4ac-0a8248e99f3f", null, "Admin", "ADMIN" },
                    { "5365b229-f6a8-4fb8-817a-59f8597ff5f4", null, "User", "USER" },
                    { "7bcfb2d2-2600-4be6-b331-d71d000a6042", null, "Creator", "CREATOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "31cb0cfb-5810-48df-b4ac-0a8248e99f3f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5365b229-f6a8-4fb8-817a-59f8597ff5f4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7bcfb2d2-2600-4be6-b331-d71d000a6042");
        }
    }
}
