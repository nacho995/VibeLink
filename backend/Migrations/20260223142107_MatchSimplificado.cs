using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class MatchSimplificado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "Matches",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ContentId",
                table: "Matches",
                newName: "MatchingUserId");

            migrationBuilder.CreateTable(
                name: "PeopleSwipe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MatchingUserId = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeopleSwipe", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeopleSwipe");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Matches",
                newName: "SessionId");

            migrationBuilder.RenameColumn(
                name: "MatchingUserId",
                table: "Matches",
                newName: "ContentId");
        }
    }
}
