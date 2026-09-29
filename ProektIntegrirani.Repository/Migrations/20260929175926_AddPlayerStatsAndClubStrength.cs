using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProektIntegrirani.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerStatsAndClubStrength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SavePoints",
                table: "PlayerPredictions",
                newName: "Breakdown_Saves");

            migrationBuilder.RenameColumn(
                name: "GoalsConcededPoints",
                table: "PlayerPredictions",
                newName: "Breakdown_GoalsConceded");

            migrationBuilder.RenameColumn(
                name: "GoalPoints",
                table: "PlayerPredictions",
                newName: "Breakdown_Goals");

            migrationBuilder.RenameColumn(
                name: "DefensiveContributionPoints",
                table: "PlayerPredictions",
                newName: "Breakdown_DefensiveContribution");

            migrationBuilder.RenameColumn(
                name: "CleanSheetPoints",
                table: "PlayerPredictions",
                newName: "Breakdown_CleanSheet");

            migrationBuilder.RenameColumn(
                name: "CardPoints",
                table: "PlayerPredictions",
                newName: "Breakdown_Cards");

            migrationBuilder.RenameColumn(
                name: "BonusPoints",
                table: "PlayerPredictions",
                newName: "Breakdown_Bonus");

            migrationBuilder.RenameColumn(
                name: "AssistPoints",
                table: "PlayerPredictions",
                newName: "Breakdown_Assists");

            migrationBuilder.RenameColumn(
                name: "AppearancePoints",
                table: "PlayerPredictions",
                newName: "Breakdown_Appearance");

            migrationBuilder.AddColumn<int>(
                name: "Stats_Assists",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_Bonus",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_DefensiveContribution",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Stats_ExpectedAssists",
                table: "Players",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Stats_ExpectedGoals",
                table: "Players",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Stats_ExpectedGoalsConceded",
                table: "Players",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_GoalsScored",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_Minutes",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_Saves",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_Starts",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_TotalPoints",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_YellowCards",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StrengthAway",
                table: "Clubs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StrengthHome",
                table: "Clubs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stats_Assists",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Stats_Bonus",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Stats_DefensiveContribution",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Stats_ExpectedAssists",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Stats_ExpectedGoals",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Stats_ExpectedGoalsConceded",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Stats_GoalsScored",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Stats_Minutes",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Stats_Saves",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Stats_Starts",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Stats_TotalPoints",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Stats_YellowCards",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "StrengthAway",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "StrengthHome",
                table: "Clubs");

            migrationBuilder.RenameColumn(
                name: "Breakdown_Saves",
                table: "PlayerPredictions",
                newName: "SavePoints");

            migrationBuilder.RenameColumn(
                name: "Breakdown_GoalsConceded",
                table: "PlayerPredictions",
                newName: "GoalsConcededPoints");

            migrationBuilder.RenameColumn(
                name: "Breakdown_Goals",
                table: "PlayerPredictions",
                newName: "GoalPoints");

            migrationBuilder.RenameColumn(
                name: "Breakdown_DefensiveContribution",
                table: "PlayerPredictions",
                newName: "DefensiveContributionPoints");

            migrationBuilder.RenameColumn(
                name: "Breakdown_CleanSheet",
                table: "PlayerPredictions",
                newName: "CleanSheetPoints");

            migrationBuilder.RenameColumn(
                name: "Breakdown_Cards",
                table: "PlayerPredictions",
                newName: "CardPoints");

            migrationBuilder.RenameColumn(
                name: "Breakdown_Bonus",
                table: "PlayerPredictions",
                newName: "BonusPoints");

            migrationBuilder.RenameColumn(
                name: "Breakdown_Assists",
                table: "PlayerPredictions",
                newName: "AssistPoints");

            migrationBuilder.RenameColumn(
                name: "Breakdown_Appearance",
                table: "PlayerPredictions",
                newName: "AppearancePoints");
        }
    }
}
