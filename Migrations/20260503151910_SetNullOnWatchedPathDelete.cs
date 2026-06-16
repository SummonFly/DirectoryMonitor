using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryMonitor.Migrations
{
    /// <inheritdoc />
    public partial class SetNullOnWatchedPathDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventLogEntries_WatchedPaths_WatchedPathId",
                table: "EventLogEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_EventLogEntries_WatchedPaths_WatchedPathId",
                table: "EventLogEntries",
                column: "WatchedPathId",
                principalTable: "WatchedPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventLogEntries_WatchedPaths_WatchedPathId",
                table: "EventLogEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_EventLogEntries_WatchedPaths_WatchedPathId",
                table: "EventLogEntries",
                column: "WatchedPathId",
                principalTable: "WatchedPaths",
                principalColumn: "Id");
        }
    }
}
