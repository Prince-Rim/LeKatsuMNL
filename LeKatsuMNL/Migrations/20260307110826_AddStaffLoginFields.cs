using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffLoginFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLists_CommissaryInventories_ComId",
                table: "OrderLists");

            migrationBuilder.RenameColumn(
                name: "ComId",
                table: "OrderLists",
                newName: "SkuId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderLists_ComId",
                table: "OrderLists",
                newName: "IX_OrderLists_SkuId");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "StaffInformations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "StaffInformations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Privileges",
                table: "StaffInformations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "StaffArchives",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "StaffArchives",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Privileges",
                table: "StaffArchives",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CommissaryInventoryComId",
                table: "OrderLists",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SkuHeaderSkuId",
                table: "OrderLists",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SkuHeader",
                columns: table => new
                {
                    SkuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkuHeader", x => x.SkuId);
                    table.ForeignKey(
                        name: "FK_SkuHeader_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkuRecipe",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkuId = table.Column<int>(type: "int", nullable: false),
                    SkuHeaderSkuId = table.Column<int>(type: "int", nullable: false),
                    ComId = table.Column<int>(type: "int", nullable: false),
                    CommissaryInventoryComId = table.Column<int>(type: "int", nullable: false),
                    QuantityNeeded = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Uom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkuRecipe", x => x.RecipeId);
                    table.ForeignKey(
                        name: "FK_SkuRecipe_CommissaryInventories_CommissaryInventoryComId",
                        column: x => x.CommissaryInventoryComId,
                        principalTable: "CommissaryInventories",
                        principalColumn: "ComId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkuRecipe_SkuHeader_SkuHeaderSkuId",
                        column: x => x.SkuHeaderSkuId,
                        principalTable: "SkuHeader",
                        principalColumn: "SkuId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLists_CommissaryInventoryComId",
                table: "OrderLists",
                column: "CommissaryInventoryComId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLists_SkuHeaderSkuId",
                table: "OrderLists",
                column: "SkuHeaderSkuId");

            migrationBuilder.CreateIndex(
                name: "IX_SkuHeader_CategoryId",
                table: "SkuHeader",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SkuRecipe_CommissaryInventoryComId",
                table: "SkuRecipe",
                column: "CommissaryInventoryComId");

            migrationBuilder.CreateIndex(
                name: "IX_SkuRecipe_SkuHeaderSkuId",
                table: "SkuRecipe",
                column: "SkuHeaderSkuId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLists_CommissaryInventories_CommissaryInventoryComId",
                table: "OrderLists",
                column: "CommissaryInventoryComId",
                principalTable: "CommissaryInventories",
                principalColumn: "ComId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLists_CommissaryInventories_CommissaryInventoryComId",
                table: "OrderLists");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLists_SkuHeader_SkuHeaderSkuId",
                table: "OrderLists");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLists_SkuHeader_SkuId",
                table: "OrderLists");

            migrationBuilder.DropTable(
                name: "SkuRecipe");

            migrationBuilder.DropTable(
                name: "SkuHeader");

            migrationBuilder.DropIndex(
                name: "IX_OrderLists_CommissaryInventoryComId",
                table: "OrderLists");

            migrationBuilder.DropIndex(
                name: "IX_OrderLists_SkuHeaderSkuId",
                table: "OrderLists");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "StaffInformations");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "StaffInformations");

            migrationBuilder.DropColumn(
                name: "Privileges",
                table: "StaffInformations");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "StaffArchives");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "StaffArchives");

            migrationBuilder.DropColumn(
                name: "Privileges",
                table: "StaffArchives");

            migrationBuilder.DropColumn(
                name: "CommissaryInventoryComId",
                table: "OrderLists");

            migrationBuilder.DropColumn(
                name: "SkuHeaderSkuId",
                table: "OrderLists");

            migrationBuilder.RenameColumn(
                name: "SkuId",
                table: "OrderLists",
                newName: "ComId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderLists_SkuId",
                table: "OrderLists",
                newName: "IX_OrderLists_ComId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLists_CommissaryInventories_ComId",
                table: "OrderLists",
                column: "ComId",
                principalTable: "CommissaryInventories",
                principalColumn: "ComId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
