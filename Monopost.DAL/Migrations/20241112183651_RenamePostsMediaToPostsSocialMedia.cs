using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monopost.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenamePostsMediaToPostsSocialMedia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "PostMedia",
                newName: "PostsSocialMedia");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "PostsSocialMedia",
                newName: "PostMedia");
        }
    }
}
