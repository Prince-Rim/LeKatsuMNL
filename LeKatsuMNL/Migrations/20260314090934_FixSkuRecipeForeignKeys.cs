using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class FixSkuRecipeForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLists_SkuHeaders_SkuHeaderSkuId",
                table: "OrderLists");

            migrationBuilder.DropForeignKey(
                name: "FK_SkuRecipes_CommissaryInventories_CommissaryInventoryComId",
                table: "SkuRecipes");

            migrationBuilder.DropForeignKey(
                name: "FK_SkuRecipes_SkuHeaders_SkuHeaderSkuId",
                table: "SkuRecipes");

            migrationBuilder.DropIndex(
                name: "IX_SkuRecipes_CommissaryInventoryComId",
                table: "SkuRecipes");

            migrationBuilder.DropIndex(
                name: "IX_SkuRecipes_SkuHeaderSkuId",
                table: "SkuRecipes");

            migrationBuilder.DropIndex(
                name: "IX_OrderLists_SkuHeaderSkuId",
                table: "OrderLists");

            migrationBuilder.DropColumn(
                name: "CommissaryInventoryComId",
                table: "SkuRecipes");

            migrationBuilder.DropColumn(
                name: "SkuHeaderSkuId",
                table: "SkuRecipes");

            migrationBuilder.DropColumn(
                name: "SkuHeaderSkuId",
                table: "OrderLists");

            migrationBuilder.CreateIndex(
                name: "IX_SkuRecipes_ComId",
                table: "SkuRecipes",
                column: "ComId");

            migrationBuilder.CreateIndex(
                name: "IX_SkuRecipes_SkuId",
                table: "SkuRecipes",
                column: "SkuId");

            migrationBuilder.AddForeignKey(
                name: "FK_SkuRecipes_CommissaryInventories_ComId",
                table: "SkuRecipes",
                column: "ComId",
                principalTable: "CommissaryInventories",
                principalColumn: "ComId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SkuRecipes_SkuHeaders_SkuId",
                table: "SkuRecipes",
                column: "SkuId",
                principalTable: "SkuHeaders",
                principalColumn: "SkuId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkuRecipes_CommissaryInventories_ComId",
                table: "SkuRecipes");

            migrationBuilder.DropForeignKey(
                name: "FK_SkuRecipes_SkuHeaders_SkuId",
                table: "SkuRecipes");

            migrationBuilder.DropIndex(
                name: "IX_SkuRecipes_ComId",
                table: "SkuRecipes");

            migrationBuilder.DropIndex(
                name: "IX_SkuRecipes_SkuId",
                table: "SkuRecipes");

            migrationBuilder.AddColumn<int>(
                name: "CommissaryInventoryComId",
                table: "SkuRecipes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SkuHeaderSkuId",
                table: "SkuRecipes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SkuHeaderSkuId",
                table: "OrderLists",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkuRecipes_CommissaryInventoryComId",
                table: "SkuRecipes",
                column: "CommissaryInventoryComId");

            migrationBuilder.CreateIndex(
                name: "IX_SkuRecipes_SkuHeaderSkuId",
                table: "SkuRecipes",
                column: "SkuHeaderSkuId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLists_SkuHeaderSkuId",
                table: "OrderLists",
                column: "SkuHeaderSkuId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLists_SkuHeaders_SkuHeaderSkuId",
                table: "OrderLists",
                column: "SkuHeaderSkuId",
                principalTable: "SkuHeaders",
                principalColumn: "SkuId");

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
    }
}
