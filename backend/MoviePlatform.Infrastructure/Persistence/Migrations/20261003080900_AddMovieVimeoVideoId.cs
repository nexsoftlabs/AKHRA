using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieVimeoVideoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VimeoVideoId",
                table: "movies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VimeoVideoId",
                table: "movies");
        }
    }
}
