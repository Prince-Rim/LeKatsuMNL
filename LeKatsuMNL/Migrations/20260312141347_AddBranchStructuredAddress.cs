using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchStructuredAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BranchLocationAddress",
                table: "BranchLocations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "Barangay",
                table: "BranchLocations",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CityMunicipality",
                table: "BranchLocations",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchLocations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IslandGroup",
                table: "BranchLocations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "BranchLocations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "BranchLocations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StreetAddress",
                table: "BranchLocations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "BranchLocations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "BranchLocationAddress",
                table: "BranchLocationArchives",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "Barangay",
                table: "BranchLocationArchives",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CityMunicipality",
                table: "BranchLocationArchives",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchLocationArchives",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IslandGroup",
                table: "BranchLocationArchives",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "BranchLocationArchives",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "BranchLocationArchives",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StreetAddress",
                table: "BranchLocationArchives",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "BranchLocationArchives",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Barangay",
                table: "BranchLocations");

            migrationBuilder.DropColumn(
                name: "CityMunicipality",
                table: "BranchLocations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "BranchLocations");

            migrationBuilder.DropColumn(
                name: "IslandGroup",
                table: "BranchLocations");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "BranchLocations");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "BranchLocations");

            migrationBuilder.DropColumn(
                name: "StreetAddress",
                table: "BranchLocations");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "BranchLocations");

            migrationBuilder.DropColumn(
                name: "Barangay",
                table: "BranchLocationArchives");

            migrationBuilder.DropColumn(
                name: "CityMunicipality",
                table: "BranchLocationArchives");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "BranchLocationArchives");

            migrationBuilder.DropColumn(
                name: "IslandGroup",
                table: "BranchLocationArchives");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "BranchLocationArchives");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "BranchLocationArchives");

            migrationBuilder.DropColumn(
                name: "StreetAddress",
                table: "BranchLocationArchives");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "BranchLocationArchives");

            migrationBuilder.AlterColumn<string>(
                name: "BranchLocationAddress",
                table: "BranchLocations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "BranchLocationAddress",
                table: "BranchLocationArchives",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
