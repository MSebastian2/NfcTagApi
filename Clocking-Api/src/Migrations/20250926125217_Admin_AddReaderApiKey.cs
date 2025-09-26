using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clocking.Api.Migrations
{
    /// <inheritdoc />
    public partial class Admin_AddReaderApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Readers",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Readers_ApiKey",
                table: "Readers",
                column: "ApiKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Readers_ApiKey",
                table: "Readers");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Readers");
        }
    }
}
