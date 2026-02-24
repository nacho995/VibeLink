using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SwipePersonas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentId",
                table: "Swipes");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "Swipes",
                newName: "MatchingUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MatchingUserId",
                table: "Swipes",
                newName: "SessionId");

            migrationBuilder.AddColumn<int>(
                name: "ContentId",
                table: "Swipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
