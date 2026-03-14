using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoryAndPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "CommissaryInventories",
                newName: "SellingPrice");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Categories",
                newName: "SubCategoryNames");

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "CommissaryInventories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SubCategoryId",
                table: "CommissaryInventories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubClass",
                table: "CommissaryInventories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissaryInventories_SubCategoryId",
                table: "CommissaryInventories",
                column: "SubCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissaryInventories_SubCategories_SubCategoryId",
                table: "CommissaryInventories",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "SubCategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommissaryInventories_SubCategories_SubCategoryId",
                table: "CommissaryInventories");

            migrationBuilder.DropIndex(
                name: "IX_CommissaryInventories_SubCategoryId",
                table: "CommissaryInventories");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "CommissaryInventories");

            migrationBuilder.DropColumn(
                name: "SubCategoryId",
                table: "CommissaryInventories");

            migrationBuilder.DropColumn(
                name: "SubClass",
                table: "CommissaryInventories");

            migrationBuilder.RenameColumn(
                name: "SellingPrice",
                table: "CommissaryInventories",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "SubCategoryNames",
                table: "Categories",
                newName: "Description");
        }
    }
}
