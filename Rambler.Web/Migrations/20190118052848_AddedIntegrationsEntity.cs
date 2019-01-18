using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Web.Migrations
{
    public partial class AddedIntegrationsEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConfigurationSettings",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ConfigurationSettings",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.CreateTable(
                name: "Integrations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Integrations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Integrations",
                columns: new[] { "Id", "IsEnabled", "Name" },
                values: new object[] { 1, false, "Youtube" });

            migrationBuilder.InsertData(
                table: "Integrations",
                columns: new[] { "Id", "IsEnabled", "Name" },
                values: new object[] { 2, false, "Twitch" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Integrations");

            migrationBuilder.InsertData(
                table: "ConfigurationSettings",
                columns: new[] { "Id", "Description", "Key", "Name", "Value" },
                values: new object[] { 5, null, "Youtube:Enabled", "Youtube Enabled", null });

            migrationBuilder.InsertData(
                table: "ConfigurationSettings",
                columns: new[] { "Id", "Description", "Key", "Name", "Value" },
                values: new object[] { 6, null, "Twitch:Enabled", "Twitch Enabled", null });
        }
    }
}
