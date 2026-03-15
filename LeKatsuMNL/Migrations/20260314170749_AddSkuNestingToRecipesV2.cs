using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class AddSkuNestingToRecipesV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ComId",
                table: "SkuRecipes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TargetSkuId",
                table: "SkuRecipes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkuRecipes_TargetSkuId",
                table: "SkuRecipes",
                column: "TargetSkuId");

            migrationBuilder.AddForeignKey(
                name: "FK_SkuRecipes_SkuHeaders_TargetSkuId",
                table: "SkuRecipes",
                column: "TargetSkuId",
                principalTable: "SkuHeaders",
                principalColumn: "SkuId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkuRecipes_SkuHeaders_TargetSkuId",
                table: "SkuRecipes");

            migrationBuilder.DropIndex(
                name: "IX_SkuRecipes_TargetSkuId",
                table: "SkuRecipes");

            migrationBuilder.DropColumn(
                name: "TargetSkuId",
                table: "SkuRecipes");

            migrationBuilder.AlterColumn<int>(
                name: "ComId",
                table: "SkuRecipes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
