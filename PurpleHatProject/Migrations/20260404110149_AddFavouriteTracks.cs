using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurpleHatProject.Migrations
{
    /// <inheritdoc />
    public partial class AddFavouriteTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavouriteTracks",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AudioUrl = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavouriteTracks", x => new { x.UserId, x.AudioUrl });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavouriteTracks");
        }
    }
}
