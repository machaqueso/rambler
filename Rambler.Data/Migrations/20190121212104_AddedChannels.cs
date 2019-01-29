using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Web.Migrations
{
    public partial class AddedChannels : Migration
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Channels");
        }
    }
}
