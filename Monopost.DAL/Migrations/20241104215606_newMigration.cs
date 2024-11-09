using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monopost.DAL.Migrations
{
    /// <inheritdoc />
    public partial class newMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewSocialMediaName",
                table: "PostsSocialMedia",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE \"PostsSocialMedia\" SET \"NewSocialMediaName\" = CAST(\"SocialMediaName\" AS integer)"
            );

            migrationBuilder.DropColumn(
                name: "SocialMediaName",
                table: "PostsSocialMedia"
            );

            migrationBuilder.RenameColumn(
                name: "NewSocialMediaName",
                table: "PostsSocialMedia",
                newName: "SocialMediaName"
            );
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SocialMediaName",
                table: "PostsSocialMedia",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
