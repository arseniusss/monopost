using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monopost.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUnnecessaryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Templates_Users_AuthorId",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_AuthorId",
                table: "Templates");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Templates",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_UserId",
                table: "Templates",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Templates_Users_UserId",
                table: "Templates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Templates_Users_UserId",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_UserId",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Templates");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_AuthorId",
                table: "Templates",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Templates_Users_AuthorId",
                table: "Templates",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
