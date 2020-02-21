using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Data.Migrations
{
    public partial class AddedUpdateDateToIntegrationEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "Integrations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "Integrations");
        }
    }
}
