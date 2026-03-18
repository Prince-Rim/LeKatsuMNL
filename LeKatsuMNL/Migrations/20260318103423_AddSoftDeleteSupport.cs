using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "VendorInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "SupplyOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "StaffInformations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "SkuHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "RestaurantItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "OrderInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "CommissaryInventories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "BranchManagers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "BranchLocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "AdminAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "VendorInfos");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "SupplyOrders");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "StaffInformations");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "SkuHeaders");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "RestaurantItems");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "OrderInfos");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "CommissaryInventories");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "BranchManagers");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "BranchLocations");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "AdminAccounts");
        }
    }
}
