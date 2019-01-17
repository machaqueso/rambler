using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Web.Migrations
{
    public partial class SeedConfigurationSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ConfigurationSettings",
                columns: new[] { "Id", "Description", "Key", "Name", "Value" },
                values: new object[] { 1, null, "Authentication:Google:ClientId", "Youtube client id", null });

            migrationBuilder.InsertData(
                table: "ConfigurationSettings",
                columns: new[] { "Id", "Description", "Key", "Name", "Value" },
                values: new object[] { 2, null, "Authentication:Google:ClientSecret", "Youtube client secret", null });

            migrationBuilder.InsertData(
                table: "ConfigurationSettings",
                columns: new[] { "Id", "Description", "Key", "Name", "Value" },
                values: new object[] { 3, null, "Authentication:Twitch:ClientId", "Twitch client id", null });

            migrationBuilder.InsertData(
                table: "ConfigurationSettings",
                columns: new[] { "Id", "Description", "Key", "Name", "Value" },
                values: new object[] { 4, null, "Authentication:Twitch:ClientSecret", "Twitch client secret", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConfigurationSettings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ConfigurationSettings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ConfigurationSettings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ConfigurationSettings",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
