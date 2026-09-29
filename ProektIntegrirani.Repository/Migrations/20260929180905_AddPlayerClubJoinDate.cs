using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProektIntegrirani.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerClubJoinDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "Stats_ClubJoinDate",
                table: "Players",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stats_ClubJoinDate",
                table: "Players");
        }
    }
}
