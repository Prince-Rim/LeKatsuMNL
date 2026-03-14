using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderCommentAttribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "BranchManagerId",
                table: "OrderComments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AdminAccountId",
                table: "OrderComments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderComments_AdminAccountId",
                table: "OrderComments",
                column: "AdminAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderComments_AdminAccounts_AdminAccountId",
                table: "OrderComments",
                column: "AdminAccountId",
                principalTable: "AdminAccounts",
                principalColumn: "ManagerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderComments_AdminAccounts_AdminAccountId",
                table: "OrderComments");

            migrationBuilder.DropIndex(
                name: "IX_OrderComments_AdminAccountId",
                table: "OrderComments");

            migrationBuilder.DropColumn(
                name: "AdminAccountId",
                table: "OrderComments");

            migrationBuilder.AlterColumn<int>(
                name: "BranchManagerId",
                table: "OrderComments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
