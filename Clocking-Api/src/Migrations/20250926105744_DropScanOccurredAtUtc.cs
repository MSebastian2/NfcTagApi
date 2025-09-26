using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clocking.Api.Migrations
{
    /// <inheritdoc />
    public partial class DropScanOccurredAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OccurredAtUtc",
                table: "Scans");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OccurredAtUtc",
                table: "Scans",
                type: "TEXT",
                nullable: false,
                defaultValue: DateTime.MinValue);
        }
    }
}
