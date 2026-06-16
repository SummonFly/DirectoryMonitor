using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryMonitor.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRuleExecutionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RuleExecutionLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RuleExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    EventPath = table.Column<string>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", nullable: false),
                    ExecutionTimeMs = table.Column<int>(type: "INTEGER", nullable: false),
                    RuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    RuleName = table.Column<string>(type: "TEXT", nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleExecutionLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RuleExecutionLogs_RuleId",
                table: "RuleExecutionLogs",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleExecutionLogs_Timestamp",
                table: "RuleExecutionLogs",
                column: "Timestamp");
        }
    }
}
