using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Monopost.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CredentialsAreNowEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Credentials\" ALTER COLUMN \"CredentialType\" TYPE integer USING \"CredentialType\"::integer;");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CredentialType",
                table: "Credentials",
                type: "text",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}