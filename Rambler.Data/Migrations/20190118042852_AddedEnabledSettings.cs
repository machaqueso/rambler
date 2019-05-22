using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Web.Migrations
{
    public partial class AddedEnabledSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ConfigurationSettings",
                columns: new[] { "Id", "Description", "Key", "Name", "Value" },
                values: new object[] { 5, null, "Youtube:Enabled", "Youtube Enabled", null });

            migrationBuilder.InsertData(
                table: "ConfigurationSettings",
                columns: new[] { "Id", "Description", "Key", "Name", "Value" },
                values: new object[] { 6, null, "Twitch:Enabled", "Twitch Enabled", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConfigurationSettings",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ConfigurationSettings",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
