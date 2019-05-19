using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Data.Migrations
{
    public partial class ConsolidatedMigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    MinimumReputation = table.Column<int>(nullable: true),
                    MaximumReputation = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationSettings", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(nullable: false),
                    Author = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: true),
                    SourceMessageId = table.Column<string>(nullable: true),
                    SourceAuthorId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    LastLoginDate = table.Column<DateTime>(nullable: true),
                    FailedLogins = table.Column<int>(nullable: false),
                    IsLocked = table.Column<bool>(nullable: false),
                    MustChangePassword = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessTokens",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiSource = table.Column<string>(nullable: true),
                    access_token = table.Column<string>(nullable: true),
                    token_type = table.Column<string>(nullable: true),
                    expires_in = table.Column<int>(nullable: false),
                    refresh_token = table.Column<string>(nullable: true),
                    ExpirationDate = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalAccount",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiSource = table.Column<string>(nullable: true),
                    ReferenceId = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalAccount_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Role_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Channels",
                columns: new[] { "Id", "Description", "MaximumReputation", "MinimumReputation", "Name" },
                values: new object[] { 1, null, null, null, "All" });

            migrationBuilder.InsertData(
                table: "Channels",
                columns: new[] { "Id", "Description", "MaximumReputation", "MinimumReputation", "Name" },
                values: new object[] { 2, null, null, null, "Reader" });

            migrationBuilder.InsertData(
                table: "Channels",
                columns: new[] { "Id", "Description", "MaximumReputation", "MinimumReputation", "Name" },
                values: new object[] { 3, null, null, null, "OBS" });

            migrationBuilder.InsertData(
                table: "Channels",
                columns: new[] { "Id", "Description", "MaximumReputation", "MinimumReputation", "Name" },
                values: new object[] { 4, null, null, null, "TTS" });

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

            migrationBuilder.InsertData(
                table: "Integrations",
                columns: new[] { "Id", "IsEnabled", "Name" },
                values: new object[] { 1, false, "Youtube" });

            migrationBuilder.InsertData(
                table: "Integrations",
                columns: new[] { "Id", "IsEnabled", "Name" },
                values: new object[] { 2, false, "Twitch" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FailedLogins", "IsLocked", "LastLoginDate", "MustChangePassword", "PasswordHash", "UserName" },
                values: new object[] { 1, null, 0, false, null, true, null, "Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessTokens_UserId",
                table: "AccessTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalAccount_UserId",
                table: "ExternalAccount",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_UserId",
                table: "Role",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessTokens");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "ConfigurationSettings");

            migrationBuilder.DropTable(
                name: "ExternalAccount");

            migrationBuilder.DropTable(
                name: "Integrations");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
