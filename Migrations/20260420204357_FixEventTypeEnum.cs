using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryMonitor.Migrations
{
    /// <inheritdoc />
    public partial class FixEventTypeEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventLogEntries_WatchedPaths_WatchedPathId",
                table: "EventLogEntries");

            migrationBuilder.CreateTable(
                name: "RuleExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    RuleName = table.Column<string>(type: "TEXT", nullable: false),
                    EventPath = table.Column<string>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    ExecutionTimeMs = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleExecutionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", nullable: false),
                    DefinitionJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RuleExecutionLogs_RuleId",
                table: "RuleExecutionLogs",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleExecutionLogs_Timestamp",
                table: "RuleExecutionLogs",
                column: "Timestamp");

            migrationBuilder.AddForeignKey(
                name: "FK_EventLogEntries_WatchedPaths_WatchedPathId",
                table: "EventLogEntries",
                column: "WatchedPathId",
                principalTable: "WatchedPaths",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventLogEntries_WatchedPaths_WatchedPathId",
                table: "EventLogEntries");

            migrationBuilder.DropTable(
                name: "RuleExecutionLogs");

            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.AddForeignKey(
                name: "FK_EventLogEntries_WatchedPaths_WatchedPathId",
                table: "EventLogEntries",
                column: "WatchedPathId",
                principalTable: "WatchedPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
