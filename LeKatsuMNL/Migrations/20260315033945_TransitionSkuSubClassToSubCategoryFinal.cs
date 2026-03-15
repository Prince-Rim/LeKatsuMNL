using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class TransitionSkuSubClassToSubCategoryFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "SubClass",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "SubClass",
                table: "CommissaryInventories");

            migrationBuilder.AddColumn<int>(
                name: "SubCategoryId",
                table: "SkuHeaders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkuHeaders_SubCategoryId",
                table: "SkuHeaders",
                column: "SubCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_SkuHeaders_SubCategories_SubCategoryId",
                table: "SkuHeaders",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "SubCategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkuHeaders_SubCategories_SubCategoryId",
                table: "SkuHeaders");

            migrationBuilder.DropIndex(
                name: "IX_SkuHeaders_SubCategoryId",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "SubCategoryId",
                table: "SkuHeaders");

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "SkuHeaders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubClass",
                table: "SkuHeaders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubClass",
                table: "CommissaryInventories",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
