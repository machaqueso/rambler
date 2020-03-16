using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Data.Migrations
{
    public partial class DiscordSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Integrations",
                keyColumn: "Id",
                keyValue: 1,
                column: "DisplayOrder",
                value: 30);

            migrationBuilder.UpdateData(
                table: "Integrations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DisplayOrder", "IsVisible" },
                values: new object[] { 10, true });

            migrationBuilder.InsertData(
                table: "Integrations",
                columns: new[] { "Id", "DisplayOrder", "IsEnabled", "IsVisible", "Name", "Status", "UpdateDate" },
                values: new object[] { 3, 20, false, true, "Discord", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Integrations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Integrations",
                keyColumn: "Id",
                keyValue: 1,
                column: "DisplayOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Integrations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DisplayOrder", "IsVisible" },
                values: new object[] { 0, false });
        }
    }
}
