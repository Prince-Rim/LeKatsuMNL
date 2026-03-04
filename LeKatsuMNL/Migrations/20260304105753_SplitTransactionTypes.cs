using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class SplitTransactionTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantTransactions_TransactionTypes_TtId",
                table: "RestaurantTransactions");

            migrationBuilder.DropTable(
                name: "TransactionTypes");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "InventoryTransactions");

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "InventoryTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "InvTransactionTypes",
                columns: table => new
                {
                    TypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvTransactionTypes", x => x.TypeId);
                });

            migrationBuilder.CreateTable(
                name: "ResTransactionTypes",
                columns: table => new
                {
                    TtId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResTransactionTypes", x => x.TtId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TypeId",
                table: "InventoryTransactions",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_InvTransactionTypes_TypeId",
                table: "InventoryTransactions",
                column: "TypeId",
                principalTable: "InvTransactionTypes",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantTransactions_ResTransactionTypes_TtId",
                table: "RestaurantTransactions",
                column: "TtId",
                principalTable: "ResTransactionTypes",
                principalColumn: "TtId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_InvTransactionTypes_TypeId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantTransactions_ResTransactionTypes_TtId",
                table: "RestaurantTransactions");

            migrationBuilder.DropTable(
                name: "InvTransactionTypes");

            migrationBuilder.DropTable(
                name: "ResTransactionTypes");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_TypeId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "InventoryTransactions");

            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "InventoryTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TransactionTypes",
                columns: table => new
                {
                    TtId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTypes", x => x.TtId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantTransactions_TransactionTypes_TtId",
                table: "RestaurantTransactions",
                column: "TtId",
                principalTable: "TransactionTypes",
                principalColumn: "TtId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
