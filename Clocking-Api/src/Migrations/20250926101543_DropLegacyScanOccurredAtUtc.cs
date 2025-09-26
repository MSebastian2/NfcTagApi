using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clocking.Api.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyScanOccurredAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TagUid = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Readers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Readers_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Scans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkerId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReaderId = table.Column<int>(type: "INTEGER", nullable: false),
                    Uid = table.Column<string>(type: "TEXT", nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    WhenUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Origin = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scans_Readers_ReaderId",
                        column: x => x.ReaderId,
                        principalTable: "Readers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Scans_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkerId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EndUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    StartReaderId = table.Column<int>(type: "INTEGER", nullable: true),
                    EndReaderId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkSessions_Readers_EndReaderId",
                        column: x => x.EndReaderId,
                        principalTable: "Readers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkSessions_Readers_StartReaderId",
                        column: x => x.StartReaderId,
                        principalTable: "Readers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkSessions_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Code",
                table: "Locations",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Readers_Code",
                table: "Readers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Readers_LocationId",
                table: "Readers",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Scans_ReaderId",
                table: "Scans",
                column: "ReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Scans_WhenUtc",
                table: "Scans",
                column: "WhenUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Scans_WorkerId",
                table: "Scans",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_TagUid",
                table: "Workers",
                column: "TagUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkSessions_EndReaderId",
                table: "WorkSessions",
                column: "EndReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSessions_StartReaderId",
                table: "WorkSessions",
                column: "StartReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSessions_WorkerId",
                table: "WorkSessions",
                column: "WorkerId",
                unique: true,
                filter: "\"EndUtc\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Scans");

            migrationBuilder.DropTable(
                name: "WorkSessions");

            migrationBuilder.DropTable(
                name: "Readers");

            migrationBuilder.DropTable(
                name: "Workers");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
