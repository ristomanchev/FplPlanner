using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FplPlanner.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddEtlSyncLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EtlSyncLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobName = table.Column<string>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    ClubsLoaded = table.Column<int>(type: "INTEGER", nullable: false),
                    GameweeksLoaded = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayersLoaded = table.Column<int>(type: "INTEGER", nullable: false),
                    FixturesLoaded = table.Column<int>(type: "INTEGER", nullable: false),
                    PredictionRecalculationQueued = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtlSyncLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EtlSyncLogs");
        }
    }
}
