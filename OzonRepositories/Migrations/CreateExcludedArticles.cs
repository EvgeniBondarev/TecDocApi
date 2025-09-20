using Microsoft.EntityFrameworkCore.Migrations;

namespace OzonRepositories.Migrations;

public partial class CreateExcludedArticles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ExcludedArticles",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Article = table.Column<string>(maxLength: 100, nullable: false),
                OzonClientId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExcludedArticles", x => x.Id);
                table.ForeignKey(
                    name: "FK_ExcludedArticles_OzonClients_OzonClientId",
                    column: x => x.OzonClientId,
                    principalTable: "OzonClients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ExcludedArticles_OzonClientId",
            table: "ExcludedArticles",
            column: "OzonClientId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ExcludedArticles");
    }
}
