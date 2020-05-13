using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Data.Migrations
{
    public partial class AddedEmoticons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Emoticons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceId = table.Column<string>(nullable: true),
                    Regex = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    ApiSource = table.Column<string>(nullable: true),
                    ChannelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emoticons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emoticons_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Emoticons_ChannelId",
                table: "Emoticons",
                column: "ChannelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Emoticons");
        }
    }
}
