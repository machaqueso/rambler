using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Data.Migrations
{
    public partial class AddedInfractions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageInfractions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageId = table.Column<int>(nullable: false),
                    InfractionType = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageInfractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageInfractions_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageInfractions_MessageId",
                table: "MessageInfractions",
                column: "MessageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageInfractions");
        }
    }
}
