using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class AddItemTypeToCommissaryInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "CommissaryInventories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "CommissaryInventories");
        }
    }
}
