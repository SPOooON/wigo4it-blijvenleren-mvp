using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlijvenLeren.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentOwnershipMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorIdentityName",
                table: "comments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorIdentityName",
                table: "comments");
        }
    }
}
