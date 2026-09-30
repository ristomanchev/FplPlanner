using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FplPlanner.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clubs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FplId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clubs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gameweeks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Deadline = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsFinished = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gameweeks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Managers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FplEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamName = table.Column<string>(type: "TEXT", nullable: false),
                    ManagerName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Bank = table.Column<double>(type: "REAL", nullable: false),
                    FreeTransfers = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Managers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FplId = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    WebName = table.Column<string>(type: "TEXT", nullable: false),
                    Position = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ChanceOfPlaying = table.Column<int>(type: "INTEGER", nullable: true),
                    News = table.Column<string>(type: "TEXT", nullable: true),
                    ClubId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fixtures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FplId = table.Column<int>(type: "INTEGER", nullable: false),
                    KickoffTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HomeScore = table.Column<int>(type: "INTEGER", nullable: true),
                    AwayScore = table.Column<int>(type: "INTEGER", nullable: true),
                    IsFinished = table.Column<bool>(type: "INTEGER", nullable: false),
                    GameweekId = table.Column<Guid>(type: "TEXT", nullable: true),
                    HomeClubId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AwayClubId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fixtures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fixtures_Clubs_AwayClubId",
                        column: x => x.AwayClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fixtures_Clubs_HomeClubId",
                        column: x => x.HomeClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fixtures_Gameweeks_GameweekId",
                        column: x => x.GameweekId,
                        principalTable: "Gameweeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PlayerPredictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModelType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ExpectedMinutes = table.Column<double>(type: "REAL", nullable: false),
                    ExpectedPoints = table.Column<double>(type: "REAL", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AppearancePoints = table.Column<double>(type: "REAL", nullable: false),
                    GoalPoints = table.Column<double>(type: "REAL", nullable: false),
                    AssistPoints = table.Column<double>(type: "REAL", nullable: false),
                    CleanSheetPoints = table.Column<double>(type: "REAL", nullable: false),
                    GoalsConcededPoints = table.Column<double>(type: "REAL", nullable: false),
                    SavePoints = table.Column<double>(type: "REAL", nullable: false),
                    DefensiveContributionPoints = table.Column<double>(type: "REAL", nullable: false),
                    BonusPoints = table.Column<double>(type: "REAL", nullable: false),
                    CardPoints = table.Column<double>(type: "REAL", nullable: false),
                    PlayerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameweekId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerPredictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerPredictions_Gameweeks_GameweekId",
                        column: x => x.GameweekId,
                        principalTable: "Gameweeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerPredictions_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SquadPicks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SquadPosition = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCaptain = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsViceCaptain = table.Column<bool>(type: "INTEGER", nullable: false),
                    ManagerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameweekId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SquadPicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SquadPicks_Gameweeks_GameweekId",
                        column: x => x.GameweekId,
                        principalTable: "Gameweeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SquadPicks_Managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Managers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SquadPicks_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_FplId",
                table: "Clubs",
                column: "FplId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_AwayClubId",
                table: "Fixtures",
                column: "AwayClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_FplId",
                table: "Fixtures",
                column: "FplId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_GameweekId",
                table: "Fixtures",
                column: "GameweekId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_HomeClubId",
                table: "Fixtures",
                column: "HomeClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Gameweeks_Number",
                table: "Gameweeks",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_FplEntryId",
                table: "Managers",
                column: "FplEntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPredictions_GameweekId",
                table: "PlayerPredictions",
                column: "GameweekId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPredictions_PlayerId_GameweekId_ModelType",
                table: "PlayerPredictions",
                columns: new[] { "PlayerId", "GameweekId", "ModelType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_ClubId",
                table: "Players",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_FplId",
                table: "Players",
                column: "FplId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SquadPicks_GameweekId",
                table: "SquadPicks",
                column: "GameweekId");

            migrationBuilder.CreateIndex(
                name: "IX_SquadPicks_ManagerId_GameweekId_PlayerId",
                table: "SquadPicks",
                columns: new[] { "ManagerId", "GameweekId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SquadPicks_PlayerId",
                table: "SquadPicks",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fixtures");

            migrationBuilder.DropTable(
                name: "PlayerPredictions");

            migrationBuilder.DropTable(
                name: "SquadPicks");

            migrationBuilder.DropTable(
                name: "Gameweeks");

            migrationBuilder.DropTable(
                name: "Managers");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Clubs");
        }
    }
}
