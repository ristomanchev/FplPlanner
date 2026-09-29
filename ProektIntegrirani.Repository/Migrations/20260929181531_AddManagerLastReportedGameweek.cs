using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProektIntegrirani.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerLastReportedGameweek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastReportedGameweek",
                table: "Managers",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReportedGameweek",
                table: "Managers");
        }
    }
}
