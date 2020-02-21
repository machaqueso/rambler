using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Data.Migrations
{
    public partial class AddedIntegrationStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Integrations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Integrations");
        }
    }
}
