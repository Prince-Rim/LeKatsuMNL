using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialTrackingToStockIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "SupplyOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Unpaid");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "SupplyLists",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "InventoryTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "InventoryTransactions",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "InventoryTransactions",
                type: "decimal(18,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "SupplyOrders");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "SupplyLists");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "InventoryTransactions");
        }
    }
}
