using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRejectItemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComId",
                table: "RejectItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SkuId",
                table: "RejectItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RejectItems_ComId",
                table: "RejectItems",
                column: "ComId");

            migrationBuilder.CreateIndex(
                name: "IX_RejectItems_SkuId",
                table: "RejectItems",
                column: "SkuId");

            migrationBuilder.AddForeignKey(
                name: "FK_RejectItems_CommissaryInventories_ComId",
                table: "RejectItems",
                column: "ComId",
                principalTable: "CommissaryInventories",
                principalColumn: "ComId");

            migrationBuilder.AddForeignKey(
                name: "FK_RejectItems_SkuHeaders_SkuId",
                table: "RejectItems",
                column: "SkuId",
                principalTable: "SkuHeaders",
                principalColumn: "SkuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RejectItems_CommissaryInventories_ComId",
                table: "RejectItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RejectItems_SkuHeaders_SkuId",
                table: "RejectItems");

            migrationBuilder.DropIndex(
                name: "IX_RejectItems_ComId",
                table: "RejectItems");

            migrationBuilder.DropIndex(
                name: "IX_RejectItems_SkuId",
                table: "RejectItems");

            migrationBuilder.DropColumn(
                name: "ComId",
                table: "RejectItems");

            migrationBuilder.DropColumn(
                name: "SkuId",
                table: "RejectItems");
        }
    }
}
