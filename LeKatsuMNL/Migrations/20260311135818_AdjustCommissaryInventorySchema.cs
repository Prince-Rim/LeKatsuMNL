using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class AdjustCommissaryInventorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Yield",
                table: "CommissaryInventories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "PriceId",
                table: "CommissaryInventories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReorderValue",
                table: "CommissaryInventories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SkuId",
                table: "CommissaryInventories",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Yield",
                table: "CommissaryArchives",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "PriceId",
                table: "CommissaryArchives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReorderValue",
                table: "CommissaryArchives",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SkuId",
                table: "CommissaryArchives",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissaryInventories_SkuId",
                table: "CommissaryInventories",
                column: "SkuId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_CommissaryInventories_SkuHeaders_SkuId",
                table: "CommissaryInventories",
                column: "SkuId",
                principalTable: "SkuHeaders",
                principalColumn: "SkuId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommissaryArchives_SkuHeaders_SkuId",
                table: "CommissaryArchives");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissaryInventories_SkuHeaders_SkuId",
                table: "CommissaryInventories");

            migrationBuilder.DropIndex(
                name: "IX_CommissaryInventories_SkuId",
                table: "CommissaryInventories");

            migrationBuilder.DropIndex(
                name: "IX_CommissaryArchives_SkuId",
                table: "CommissaryArchives");

            migrationBuilder.DropColumn(
                name: "PriceId",
                table: "CommissaryInventories");

            migrationBuilder.DropColumn(
                name: "ReorderValue",
                table: "CommissaryInventories");

            migrationBuilder.DropColumn(
                name: "SkuId",
                table: "CommissaryInventories");

            migrationBuilder.DropColumn(
                name: "PriceId",
                table: "CommissaryArchives");

            migrationBuilder.DropColumn(
                name: "ReorderValue",
                table: "CommissaryArchives");

            migrationBuilder.DropColumn(
                name: "SkuId",
                table: "CommissaryArchives");

            migrationBuilder.AlterColumn<decimal>(
                name: "Yield",
                table: "CommissaryInventories",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Yield",
                table: "CommissaryArchives",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
