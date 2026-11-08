using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookshelf.API.Migrations
{
    /// <inheritdoc />
    public partial class ObrazyWBlobStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarImageData",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CustomCoverImageData",
                table: "UserBooks");

            migrationBuilder.AddColumn<string>(
                name: "AvatarBlobName",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomCoverBlobName",
                table: "UserBooks",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
            // Istniejące konta nie mogą wskazywać na usunięty endpoint i usunięte bajty avatara
            migrationBuilder.Sql("UPDATE Users SET AvatarUrl = NULL WHERE AvatarUrl = '/api/users/me/avatar/image'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarBlobName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CustomCoverBlobName",
                table: "UserBooks");

            migrationBuilder.AddColumn<byte[]>(
                name: "AvatarImageData",
                table: "Users",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "CustomCoverImageData",
                table: "UserBooks",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
