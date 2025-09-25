using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clocking.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class UniqueOpenSessionPerWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReaderId",
                table: "Scans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Scans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "WhenUtc",
                table: "Scans",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Scans");

            migrationBuilder.DropColumn(
                name: "WhenUtc",
                table: "Scans");

            migrationBuilder.AlterColumn<int>(
                name: "ReaderId",
                table: "Scans",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
