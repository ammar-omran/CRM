using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Ticketing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by_name",
                schema: "ticketing",
                table: "ticket_comments",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_admin",
                schema: "ticketing",
                table: "ticket_comments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by_name",
                schema: "ticketing",
                table: "ticket_comments");

            migrationBuilder.DropColumn(
                name: "is_admin",
                schema: "ticketing",
                table: "ticket_comments");
        }
    }
}
