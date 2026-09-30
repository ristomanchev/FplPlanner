using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProektIntegrirani.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SquadPicks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "SquadPicks",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateLastModified",
                table: "SquadPicks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "SquadPicks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Managers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Managers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateLastModified",
                table: "Managers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Managers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ApiClients",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "ApiClients",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateLastModified",
                table: "ApiClients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "ApiClients",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SquadPicks");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "SquadPicks");

            migrationBuilder.DropColumn(
                name: "DateLastModified",
                table: "SquadPicks");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "SquadPicks");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "DateLastModified",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ApiClients");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "ApiClients");

            migrationBuilder.DropColumn(
                name: "DateLastModified",
                table: "ApiClients");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "ApiClients");
        }
    }
}
