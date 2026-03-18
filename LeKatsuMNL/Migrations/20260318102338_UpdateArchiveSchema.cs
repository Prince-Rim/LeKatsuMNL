using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateArchiveSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommissaryArchives_SkuHeaders_SkuId",
                table: "CommissaryArchives");

            migrationBuilder.DropIndex(
                name: "IX_CommissaryArchives_SkuId",
                table: "CommissaryArchives");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "CommissaryArchives");

            migrationBuilder.DropColumn(
                name: "ReorderValue",
                table: "CommissaryArchives");

            migrationBuilder.RenameColumn(
                name: "PriceId",
                table: "CommissaryArchives",
                newName: "SubCategoryId");

            migrationBuilder.AlterColumn<int>(
                name: "VendorId",
                table: "CommissaryArchives",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Stock",
                table: "CommissaryArchives",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "CommissaryArchives",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsRepack",
                table: "CommissaryArchives",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                table: "CommissaryArchives",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "BranchManagerArchives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "BranchManagerArchives",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactNum",
                table: "AdminArchives",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AdminArchives",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SkuArchives",
                columns: table => new
                {
                    SaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkuId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    PackagingType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PackagingUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PackSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Uom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsSellingPriceEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsReorderLevelEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkuArchives", x => x.SaId);
                });

            migrationBuilder.CreateTable(
                name: "VendorArchives",
                columns: table => new
                {
                    VaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    VendorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SecondVendorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SecondVendorCn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SupplierType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompanyId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CompanyAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Tin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorArchives", x => x.VaId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkuArchives");

            migrationBuilder.DropTable(
                name: "VendorArchives");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "CommissaryArchives");

            migrationBuilder.DropColumn(
                name: "IsRepack",
                table: "CommissaryArchives");

            migrationBuilder.DropColumn(
                name: "SellingPrice",
                table: "CommissaryArchives");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "BranchManagerArchives");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "BranchManagerArchives");

            migrationBuilder.DropColumn(
                name: "ContactNum",
                table: "AdminArchives");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AdminArchives");

            migrationBuilder.RenameColumn(
                name: "SubCategoryId",
                table: "CommissaryArchives",
                newName: "PriceId");

            migrationBuilder.AlterColumn<int>(
                name: "VendorId",
                table: "CommissaryArchives",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Stock",
                table: "CommissaryArchives",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "CommissaryArchives",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReorderValue",
                table: "CommissaryArchives",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissaryArchives_SkuId",
                table: "CommissaryArchives",
                column: "SkuId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissaryArchives_SkuHeaders_SkuId",
                table: "CommissaryArchives",
                column: "SkuId",
                principalTable: "SkuHeaders",
                principalColumn: "SkuId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
