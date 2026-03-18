using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientRepack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "SkuHeaders");

            migrationBuilder.AddColumn<bool>(
                name: "IsRepack",
                table: "CommissaryInventories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "IngredientRecipes",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    QuantityNeeded = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Uom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientRecipes", x => x.RecipeId);
                    table.ForeignKey(
                        name: "FK_IngredientRecipes_CommissaryInventories_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "CommissaryInventories",
                        principalColumn: "ComId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IngredientRecipes_CommissaryInventories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CommissaryInventories",
                        principalColumn: "ComId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientRecipes_MaterialId",
                table: "IngredientRecipes",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientRecipes_ParentId",
                table: "IngredientRecipes",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientRecipes");

            migrationBuilder.DropColumn(
                name: "IsRepack",
                table: "CommissaryInventories");

            migrationBuilder.AddColumn<string>(
                name: "Supplier",
                table: "SkuHeaders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
