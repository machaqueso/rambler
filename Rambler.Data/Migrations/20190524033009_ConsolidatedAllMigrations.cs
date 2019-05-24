using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Data.Migrations
{
    public partial class ConsolidatedAllMigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Source = table.Column<string>(nullable: true),
                    SourceAuthorId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Points = table.Column<int>(nullable: false),
                    Score = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

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
                name: "TwitchNotifications",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    email = table.Column<bool>(nullable: false),
                    push = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwitchNotifications", x => x.id);
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
                name: "AuthorFilters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FilterType = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    AuthorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorFilters_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthorScoreHistories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(nullable: false),
                    Value = table.Column<int>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    AuthorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorScoreHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorScoreHistories_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: true),
                    SourceMessageId = table.Column<string>(nullable: true),
                    AuthorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TwitchUsers",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    _id = table.Column<ulong>(nullable: false),
                    bio = table.Column<string>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    display_name = table.Column<string>(nullable: true),
                    email = table.Column<string>(nullable: true),
                    email_verified = table.Column<bool>(nullable: false),
                    logo = table.Column<string>(nullable: true),
                    name = table.Column<string>(nullable: true),
                    notificationsid = table.Column<int>(nullable: true),
                    partnered = table.Column<bool>(nullable: false),
                    twitter_connected = table.Column<bool>(nullable: false),
                    type = table.Column<string>(nullable: true),
                    updated_at = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwitchUsers", x => x.id);
                    table.ForeignKey(
                        name: "FK_TwitchUsers_TwitchNotifications_notificationsid",
                        column: x => x.notificationsid,
                        principalTable: "TwitchNotifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "IX_AuthorFilters_AuthorId",
                table: "AuthorFilters",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorScoreHistories_AuthorId",
                table: "AuthorScoreHistories",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AuthorId",
                table: "Messages",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_TwitchUsers_notificationsid",
                table: "TwitchUsers",
                column: "notificationsid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessTokens");

            migrationBuilder.DropTable(
                name: "AuthorFilters");

            migrationBuilder.DropTable(
                name: "AuthorScoreHistories");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "ConfigurationSettings");

            migrationBuilder.DropTable(
                name: "Integrations");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "TwitchUsers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "TwitchNotifications");
        }
    }
}
