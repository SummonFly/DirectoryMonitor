using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryMonitor.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WatchedPaths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IncludeSubdirectories = table.Column<bool>(type: "INTEGER", nullable: false),
                    FileExtensionsFilter = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchedPaths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventLogEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", nullable: false),
                    OldPath = table.Column<string>(type: "TEXT", nullable: true),
                    WatchedPathId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLogEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventLogEntries_WatchedPaths_WatchedPathId",
                        column: x => x.WatchedPathId,
                        principalTable: "WatchedPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventLogEntries_Path",
                table: "EventLogEntries",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogEntries_Timestamp",
                table: "EventLogEntries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogEntries_WatchedPathId",
                table: "EventLogEntries",
                column: "WatchedPathId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventLogEntries");

            migrationBuilder.DropTable(
                name: "WatchedPaths");
        }
    }
}
