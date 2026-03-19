using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchivedToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Categories");
        }
    }
}
