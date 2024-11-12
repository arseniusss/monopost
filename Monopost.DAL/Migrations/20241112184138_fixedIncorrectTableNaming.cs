using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monopost.DAL.Migrations
{
    /// <inheritdoc />
    public partial class fixedIncorrectTableNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostMedia_Posts_PostId",
                table: "PostMedia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostMedia",
                table: "PostMedia");

            migrationBuilder.RenameTable(
                name: "PostMedia",
                newName: "PostsSocialMedia");

            migrationBuilder.RenameIndex(
                name: "IX_PostMedia_PostId",
                table: "PostsSocialMedia",
                newName: "IX_PostsSocialMedia_PostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostsSocialMedia",
                table: "PostsSocialMedia",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostsSocialMedia_Posts_PostId",
                table: "PostsSocialMedia",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "PostId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostsSocialMedia_Posts_PostId",
                table: "PostsSocialMedia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostsSocialMedia",
                table: "PostsSocialMedia");

            migrationBuilder.RenameTable(
                name: "PostsSocialMedia",
                newName: "PostMedia");

            migrationBuilder.RenameIndex(
                name: "IX_PostsSocialMedia_PostId",
                table: "PostMedia",
                newName: "IX_PostMedia_PostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostMedia",
                table: "PostMedia",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostMedia_Posts_PostId",
                table: "PostMedia",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "PostId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
