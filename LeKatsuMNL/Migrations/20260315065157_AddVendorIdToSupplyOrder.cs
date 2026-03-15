using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorIdToSupplyOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupplyId",
                table: "SupplyOrderArchives");

            migrationBuilder.AddColumn<int>(
                name: "VendorId",
                table: "SupplyOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VendorId",
                table: "SupplyOrderArchives",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SellingPrice",
                table: "SkuHeaders",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplyOrders_VendorId",
                table: "SupplyOrders",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyOrderArchives_VendorId",
                table: "SupplyOrderArchives",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyOrderArchives_VendorInfos_VendorId",
                table: "SupplyOrderArchives",
                column: "VendorId",
                principalTable: "VendorInfos",
                principalColumn: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyOrders_VendorInfos_VendorId",
                table: "SupplyOrders",
                column: "VendorId",
                principalTable: "VendorInfos",
                principalColumn: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplyOrderArchives_VendorInfos_VendorId",
                table: "SupplyOrderArchives");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplyOrders_VendorInfos_VendorId",
                table: "SupplyOrders");

            migrationBuilder.DropIndex(
                name: "IX_SupplyOrders_VendorId",
                table: "SupplyOrders");

            migrationBuilder.DropIndex(
                name: "IX_SupplyOrderArchives_VendorId",
                table: "SupplyOrderArchives");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "SupplyOrders");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "SupplyOrderArchives");

            migrationBuilder.AddColumn<int>(
                name: "SupplyId",
                table: "SupplyOrderArchives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "SellingPrice",
                table: "SkuHeaders",
                type: "decimal(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");
        }
    }
}
