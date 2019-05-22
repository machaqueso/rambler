using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Web.Migrations
{
    public partial class ConfigurationTableEnhancements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ConfigurationSettings",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ConfigurationSettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ConfigurationSettings");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ConfigurationSettings");
        }
    }
}
