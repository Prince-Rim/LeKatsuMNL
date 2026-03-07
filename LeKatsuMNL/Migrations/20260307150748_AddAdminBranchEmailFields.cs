using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminBranchEmailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "BranchManagers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "BranchManagerArchives",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AdminArchives",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AdminAccounts",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "BranchManagers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "BranchManagerArchives");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AdminArchives");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AdminAccounts");
        }
    }
}
