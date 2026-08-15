using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Users.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AlterCreatedAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_at_utc",
                schema: "users",
                table: "users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                schema: "users",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "updated_at_utc",
                schema: "users",
                table: "refresh_tokens",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                schema: "users",
                table: "refresh_tokens",
                newName: "created_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "users",
                table: "users",
                newName: "updated_at_utc");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "users",
                table: "users",
                newName: "created_at_utc");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "users",
                table: "refresh_tokens",
                newName: "updated_at_utc");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "users",
                table: "refresh_tokens",
                newName: "created_at_utc");
        }
    }
}
