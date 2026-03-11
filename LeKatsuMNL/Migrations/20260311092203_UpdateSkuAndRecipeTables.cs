using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSkuAndRecipeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLists_SkuHeader_SkuHeaderSkuId",
                table: "OrderLists");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLists_SkuHeader_SkuId",
                table: "OrderLists");

            migrationBuilder.DropForeignKey(
                name: "FK_SkuHeader_Categories_CategoryId",
                table: "SkuHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_SkuRecipe_CommissaryInventories_CommissaryInventoryComId",
                table: "SkuRecipe");

            migrationBuilder.DropForeignKey(
                name: "FK_SkuRecipe_SkuHeader_SkuHeaderSkuId",
                table: "SkuRecipe");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SkuRecipe",
                table: "SkuRecipe");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SkuHeader",
                table: "SkuHeader");

            migrationBuilder.RenameTable(
                name: "SkuRecipe",
                newName: "SkuRecipes");

            migrationBuilder.RenameTable(
                name: "SkuHeader",
                newName: "SkuHeaders");

            migrationBuilder.RenameIndex(
                name: "IX_SkuRecipe_SkuHeaderSkuId",
                table: "SkuRecipes",
                newName: "IX_SkuRecipes_SkuHeaderSkuId");

            migrationBuilder.RenameIndex(
                name: "IX_SkuRecipe_CommissaryInventoryComId",
                table: "SkuRecipes",
                newName: "IX_SkuRecipes_CommissaryInventoryComId");

            migrationBuilder.RenameIndex(
                name: "IX_SkuHeader_CategoryId",
                table: "SkuHeaders",
                newName: "IX_SkuHeaders_CategoryId");

            migrationBuilder.AddColumn<bool>(
                name: "IsReorderLevelEnabled",
                table: "SkuHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSellingPriceEnabled",
                table: "SkuHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PackSize",
                table: "SkuHeaders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackagingType",
                table: "SkuHeaders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackagingUnit",
                table: "SkuHeaders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                table: "SkuHeaders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "SkuHeaders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubClass",
                table: "SkuHeaders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Supplier",
                table: "SkuHeaders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "SkuHeaders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uom",
                table: "SkuHeaders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SkuRecipes",
                table: "SkuRecipes",
                column: "RecipeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SkuHeaders",
                table: "SkuHeaders",
                column: "SkuId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLists_SkuHeaders_SkuHeaderSkuId",
                table: "OrderLists",
                column: "SkuHeaderSkuId",
                principalTable: "SkuHeaders",
                principalColumn: "SkuId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLists_SkuHeaders_SkuId",
                table: "OrderLists",
                column: "SkuId",
                principalTable: "SkuHeaders",
                principalColumn: "SkuId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SkuHeaders_Categories_CategoryId",
                table: "SkuHeaders",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SkuRecipes_CommissaryInventories_CommissaryInventoryComId",
                table: "SkuRecipes",
                column: "CommissaryInventoryComId",
                principalTable: "CommissaryInventories",
                principalColumn: "ComId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SkuRecipes_SkuHeaders_SkuHeaderSkuId",
                table: "SkuRecipes",
                column: "SkuHeaderSkuId",
                principalTable: "SkuHeaders",
                principalColumn: "SkuId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLists_SkuHeaders_SkuHeaderSkuId",
                table: "OrderLists");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLists_SkuHeaders_SkuId",
                table: "OrderLists");

            migrationBuilder.DropForeignKey(
                name: "FK_SkuHeaders_Categories_CategoryId",
                table: "SkuHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_SkuRecipes_CommissaryInventories_CommissaryInventoryComId",
                table: "SkuRecipes");

            migrationBuilder.DropForeignKey(
                name: "FK_SkuRecipes_SkuHeaders_SkuHeaderSkuId",
                table: "SkuRecipes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SkuRecipes",
                table: "SkuRecipes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SkuHeaders",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "IsReorderLevelEnabled",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "IsSellingPriceEnabled",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "PackSize",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "PackagingType",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "PackagingUnit",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "SellingPrice",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "SubClass",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "Uom",
                table: "SkuHeaders");

            migrationBuilder.RenameTable(
                name: "SkuRecipes",
                newName: "SkuRecipe");

            migrationBuilder.RenameTable(
                name: "SkuHeaders",
                newName: "SkuHeader");

            migrationBuilder.RenameIndex(
                name: "IX_SkuRecipes_SkuHeaderSkuId",
                table: "SkuRecipe",
                newName: "IX_SkuRecipe_SkuHeaderSkuId");

            migrationBuilder.RenameIndex(
                name: "IX_SkuRecipes_CommissaryInventoryComId",
                table: "SkuRecipe",
                newName: "IX_SkuRecipe_CommissaryInventoryComId");

            migrationBuilder.RenameIndex(
                name: "IX_SkuHeaders_CategoryId",
                table: "SkuHeader",
                newName: "IX_SkuHeader_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SkuRecipe",
                table: "SkuRecipe",
                column: "RecipeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SkuHeader",
                table: "SkuHeader",
                column: "SkuId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLists_SkuHeader_SkuHeaderSkuId",
                table: "OrderLists",
                column: "SkuHeaderSkuId",
                principalTable: "SkuHeader",
                principalColumn: "SkuId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLists_SkuHeader_SkuId",
                table: "OrderLists",
                column: "SkuId",
                principalTable: "SkuHeader",
                principalColumn: "SkuId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SkuHeader_Categories_CategoryId",
                table: "SkuHeader",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SkuRecipe_CommissaryInventories_CommissaryInventoryComId",
                table: "SkuRecipe",
                column: "CommissaryInventoryComId",
                principalTable: "CommissaryInventories",
                principalColumn: "ComId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SkuRecipe_SkuHeader_SkuHeaderSkuId",
                table: "SkuRecipe",
                column: "SkuHeaderSkuId",
                principalTable: "SkuHeader",
                principalColumn: "SkuId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
