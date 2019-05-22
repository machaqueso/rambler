using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Web.Migrations
{
    public partial class AddedLastLoginFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedLogins",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginDate",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "MustChangePassword",
                value: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedLogins",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "Users");
        }
    }
}
