using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniUrl.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTinyUrlTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "TinyUrls",
                type: "varchar(2000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TinyUrls",
                type: "varchar(2000)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortenedUrl",
                table: "TinyUrls",
                type: "varchar(300)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TinyUrls_ShortenedUrl",
                table: "TinyUrls",
                column: "ShortenedUrl",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TinyUrls_ShortenedUrl",
                table: "TinyUrls");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TinyUrls");

            migrationBuilder.DropColumn(
                name: "ShortenedUrl",
                table: "TinyUrls");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "TinyUrls",
                type: "varchar(500)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(2000)");
        }
    }
}
