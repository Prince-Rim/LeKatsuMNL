using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class SupportIngredientsInOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLists_CommissaryInventories_CommissaryInventoryComId",
                table: "OrderLists");

            migrationBuilder.RenameColumn(
                name: "CommissaryInventoryComId",
                table: "OrderLists",
                newName: "ComId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderLists_CommissaryInventoryComId",
                table: "OrderLists",
                newName: "IX_OrderLists_ComId");

            migrationBuilder.AlterColumn<int>(
                name: "SkuId",
                table: "OrderLists",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLists_CommissaryInventories_ComId",
                table: "OrderLists",
                column: "ComId",
                principalTable: "CommissaryInventories",
                principalColumn: "ComId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLists_CommissaryInventories_ComId",
                table: "OrderLists");

            migrationBuilder.RenameColumn(
                name: "ComId",
                table: "OrderLists",
                newName: "CommissaryInventoryComId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderLists_ComId",
                table: "OrderLists",
                newName: "IX_OrderLists_CommissaryInventoryComId");

            migrationBuilder.AlterColumn<int>(
                name: "SkuId",
                table: "OrderLists",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLists_CommissaryInventories_CommissaryInventoryComId",
                table: "OrderLists",
                column: "CommissaryInventoryComId",
                principalTable: "CommissaryInventories",
                principalColumn: "ComId");
        }
    }
}
