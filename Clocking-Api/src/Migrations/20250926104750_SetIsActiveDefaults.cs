using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clocking.Api.Migrations
{
    /// <inheritdoc />
    public partial class SetIsActiveDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkSessions_WorkerId",
                table: "WorkSessions");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Workers",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Locations",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSessions_WorkerId",
                table: "WorkSessions",
                column: "WorkerId",
                unique: true,
                filter: "EndUtc IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkSessions_WorkerId",
                table: "WorkSessions");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Workers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Locations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkSessions_WorkerId",
                table: "WorkSessions",
                column: "WorkerId",
                unique: true,
                filter: "\"EndUtc\" IS NULL");
        }
    }
}
