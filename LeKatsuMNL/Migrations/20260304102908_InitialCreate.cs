using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeKatsuMNL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminAccounts",
                columns: table => new
                {
                    ManagerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Privileges = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAccounts", x => x.ManagerId);
                });

            migrationBuilder.CreateTable(
                name: "AdminArchives",
                columns: table => new
                {
                    AaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManagerId = table.Column<int>(type: "int", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Privileges = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminArchives", x => x.AaId);
                });

            migrationBuilder.CreateTable(
                name: "BranchLocationArchives",
                columns: table => new
                {
                    BlaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BranchLocationAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchLocationArchives", x => x.BlaId);
                });

            migrationBuilder.CreateTable(
                name: "BranchLocations",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BranchLocationAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchLocations", x => x.BranchId);
                });

            migrationBuilder.CreateTable(
                name: "BranchManagerArchives",
                columns: table => new
                {
                    BmaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManagerId = table.Column<int>(type: "int", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchManagerArchives", x => x.BmaId);
                });

            migrationBuilder.CreateTable(
                name: "CashRegisters",
                columns: table => new
                {
                    CrId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartingCash = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentCash = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddedCash = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CashSales = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CashExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NonCashExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashRegisters", x => x.CrId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "CommissaryArchives",
                columns: table => new
                {
                    CaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Yield = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Uom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissaryArchives", x => x.CaId);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseTypes",
                columns: table => new
                {
                    ExpenseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseTypes", x => x.ExpenseId);
                });

            migrationBuilder.CreateTable(
                name: "ItemCategories",
                columns: table => new
                {
                    ItemCtgId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemCategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategories", x => x.ItemCtgId);
                });

            migrationBuilder.CreateTable(
                name: "ItemTranArchives",
                columns: table => new
                {
                    ItaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemTranId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTranArchives", x => x.ItaId);
                });

            migrationBuilder.CreateTable(
                name: "OrderListArchives",
                columns: table => new
                {
                    OlaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ComId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderListArchives", x => x.OlaId);
                });

            migrationBuilder.CreateTable(
                name: "ResItemsArchives",
                columns: table => new
                {
                    RiaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemCategory = table.Column<int>(type: "int", nullable: false),
                    ItemImg = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Option = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Modifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Availability = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VatExempt = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResItemsArchives", x => x.RiaId);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantArchives",
                columns: table => new
                {
                    RaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    BeginningStock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddedStock = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeductedStock = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentStock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StaffInputted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantArchives", x => x.RaId);
                });

            migrationBuilder.CreateTable(
                name: "StaffArchives",
                columns: table => new
                {
                    AsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffArchives", x => x.AsId);
                });

            migrationBuilder.CreateTable(
                name: "StaffInformations",
                columns: table => new
                {
                    StaffId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffInformations", x => x.StaffId);
                });

            migrationBuilder.CreateTable(
                name: "STimeArchives",
                columns: table => new
                {
                    StaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeIn = table.Column<TimeSpan>(type: "time", nullable: false),
                    TimeOut = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STimeArchives", x => x.StaId);
                });

            migrationBuilder.CreateTable(
                name: "SubBranchOrderArchives",
                columns: table => new
                {
                    SboId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    BManagerId = table.Column<int>(type: "int", nullable: false),
                    ListId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubBranchOrderArchives", x => x.SboId);
                });

            migrationBuilder.CreateTable(
                name: "SupplyHistoryArchives",
                columns: table => new
                {
                    ShaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SHistoryId = table.Column<int>(type: "int", nullable: false),
                    SupplyId = table.Column<int>(type: "int", nullable: false),
                    SListId = table.Column<int>(type: "int", nullable: false),
                    SupplyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceiptImg = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyHistoryArchives", x => x.ShaId);
                });

            migrationBuilder.CreateTable(
                name: "SupplyListArchives",
                columns: table => new
                {
                    SlaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SListId = table.Column<int>(type: "int", nullable: false),
                    SupplyId = table.Column<int>(type: "int", nullable: false),
                    ComId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyListArchives", x => x.SlaId);
                });

            migrationBuilder.CreateTable(
                name: "SupplyOrderArchives",
                columns: table => new
                {
                    SoaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplyId = table.Column<int>(type: "int", nullable: false),
                    SListId = table.Column<int>(type: "int", nullable: false),
                    SupplyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceiptImg = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyOrderArchives", x => x.SoaId);
                });

            migrationBuilder.CreateTable(
                name: "SupplyOrders",
                columns: table => new
                {
                    SoaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceiptImg = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyOrders", x => x.SoaId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionArchives",
                columns: table => new
                {
                    RtaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptNum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    ItemTranId = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsRefunded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionArchives", x => x.RtaId);
                });

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

            migrationBuilder.CreateTable(
                name: "VendorInfos",
                columns: table => new
                {
                    VendorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SecondVendorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SecondVendorCn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorInfos", x => x.VendorId);
                });

            migrationBuilder.CreateTable(
                name: "BranchManagers",
                columns: table => new
                {
                    BManagerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchManagers", x => x.BManagerId);
                    table.ForeignKey(
                        name: "FK_BranchManagers_BranchLocations_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchLocations",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashAddeds",
                columns: table => new
                {
                    CshAddId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrId = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashAddeds", x => x.CshAddId);
                    table.ForeignKey(
                        name: "FK_CashAddeds_CashRegisters_CrId",
                        column: x => x.CrId,
                        principalTable: "CashRegisters",
                        principalColumn: "CrId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    SubCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubCategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.SubCategoryId);
                    table.ForeignKey(
                        name: "FK_SubCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashExpenses",
                columns: table => new
                {
                    CshExpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrId = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpenseId = table.Column<int>(type: "int", nullable: false),
                    ExpenseName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ExpenseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashExpenses", x => x.CshExpId);
                    table.ForeignKey(
                        name: "FK_CashExpenses_CashRegisters_CrId",
                        column: x => x.CrId,
                        principalTable: "CashRegisters",
                        principalColumn: "CrId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashExpenses_ExpenseTypes_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "ExpenseTypes",
                        principalColumn: "ExpenseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantItems",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemCtgId = table.Column<int>(type: "int", nullable: false),
                    ItemImg = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Modifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Availability = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VatExempt = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantItems", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_RestaurantItems_ItemCategories_ItemCtgId",
                        column: x => x.ItemCtgId,
                        principalTable: "ItemCategories",
                        principalColumn: "ItemCtgId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantInventories",
                columns: table => new
                {
                    ResId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    BeginningStock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddedStock = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeductedStock = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentStock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantInventories", x => x.ResId);
                    table.ForeignKey(
                        name: "FK_RestaurantInventories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantInventories_StaffInformations_StaffId",
                        column: x => x.StaffId,
                        principalTable: "StaffInformations",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffTimeSlots",
                columns: table => new
                {
                    TimeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeIn = table.Column<TimeSpan>(type: "time", nullable: false),
                    TimeOut = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffTimeSlots", x => x.TimeId);
                    table.ForeignKey(
                        name: "FK_StaffTimeSlots_StaffInformations_StaffId",
                        column: x => x.StaffId,
                        principalTable: "StaffInformations",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplyHistories",
                columns: table => new
                {
                    SHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyHistories", x => x.SHistoryId);
                    table.ForeignKey(
                        name: "FK_SupplyHistories_SupplyOrders_SupplyId",
                        column: x => x.SupplyId,
                        principalTable: "SupplyOrders",
                        principalColumn: "SoaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TtId = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptNum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsRefunded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_RestaurantTransactions_StaffInformations_StaffId",
                        column: x => x.StaffId,
                        principalTable: "StaffInformations",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantTransactions_TransactionTypes_TtId",
                        column: x => x.TtId,
                        principalTable: "TransactionTypes",
                        principalColumn: "TtId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommissaryInventories",
                columns: table => new
                {
                    ComId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Yield = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Uom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissaryInventories", x => x.ComId);
                    table.ForeignKey(
                        name: "FK_CommissaryInventories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissaryInventories_VendorInfos_VendorId",
                        column: x => x.VendorId,
                        principalTable: "VendorInfos",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderInfos",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchManagerId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderInfos", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_OrderInfos_BranchManagers_BranchManagerId",
                        column: x => x.BranchManagerId,
                        principalTable: "BranchManagers",
                        principalColumn: "BManagerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemTransactions",
                columns: table => new
                {
                    ItemTranId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTransactions", x => x.ItemTranId);
                    table.ForeignKey(
                        name: "FK_ItemTransactions_RestaurantItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "RestaurantItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemTransactions_RestaurantTransactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "RestaurantTransactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuantityChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_CommissaryInventories_ComId",
                        column: x => x.ComId,
                        principalTable: "CommissaryInventories",
                        principalColumn: "ComId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplyLists",
                columns: table => new
                {
                    SListId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplyId = table.Column<int>(type: "int", nullable: false),
                    ComId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyLists", x => x.SListId);
                    table.ForeignKey(
                        name: "FK_SupplyLists_CommissaryInventories_ComId",
                        column: x => x.ComId,
                        principalTable: "CommissaryInventories",
                        principalColumn: "ComId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplyLists_SupplyOrders_SupplyId",
                        column: x => x.SupplyId,
                        principalTable: "SupplyOrders",
                        principalColumn: "SoaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK_Invoices_OrderInfos_OrderId",
                        column: x => x.OrderId,
                        principalTable: "OrderInfos",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderComments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    BranchManagerId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderComments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_OrderComments_BranchManagers_BranchManagerId",
                        column: x => x.BranchManagerId,
                        principalTable: "BranchManagers",
                        principalColumn: "BManagerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderComments_OrderInfos_OrderId",
                        column: x => x.OrderId,
                        principalTable: "OrderInfos",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderLists",
                columns: table => new
                {
                    ListId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ComId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLists", x => x.ListId);
                    table.ForeignKey(
                        name: "FK_OrderLists_CommissaryInventories_ComId",
                        column: x => x.ComId,
                        principalTable: "CommissaryInventories",
                        principalColumn: "ComId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLists_OrderInfos_OrderId",
                        column: x => x.OrderId,
                        principalTable: "OrderInfos",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchManagers_BranchId",
                table: "BranchManagers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CashAddeds_CrId",
                table: "CashAddeds",
                column: "CrId");

            migrationBuilder.CreateIndex(
                name: "IX_CashExpenses_CrId",
                table: "CashExpenses",
                column: "CrId");

            migrationBuilder.CreateIndex(
                name: "IX_CashExpenses_ExpenseId",
                table: "CashExpenses",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissaryInventories_CategoryId",
                table: "CommissaryInventories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissaryInventories_VendorId",
                table: "CommissaryInventories",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ComId",
                table: "InventoryTransactions",
                column: "ComId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OrderId",
                table: "Invoices",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTransactions_ItemId",
                table: "ItemTransactions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTransactions_TransactionId",
                table: "ItemTransactions",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderComments_BranchManagerId",
                table: "OrderComments",
                column: "BranchManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderComments_OrderId",
                table: "OrderComments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderInfos_BranchManagerId",
                table: "OrderInfos",
                column: "BranchManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLists_ComId",
                table: "OrderLists",
                column: "ComId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLists_OrderId",
                table: "OrderLists",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantInventories_CategoryId",
                table: "RestaurantInventories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantInventories_StaffId",
                table: "RestaurantInventories",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantItems_ItemCtgId",
                table: "RestaurantItems",
                column: "ItemCtgId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTransactions_StaffId",
                table: "RestaurantTransactions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTransactions_TtId",
                table: "RestaurantTransactions",
                column: "TtId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffTimeSlots_StaffId",
                table: "StaffTimeSlots",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CategoryId",
                table: "SubCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyHistories_SupplyId",
                table: "SupplyHistories",
                column: "SupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyLists_ComId",
                table: "SupplyLists",
                column: "ComId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyLists_SupplyId",
                table: "SupplyLists",
                column: "SupplyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAccounts");

            migrationBuilder.DropTable(
                name: "AdminArchives");

            migrationBuilder.DropTable(
                name: "BranchLocationArchives");

            migrationBuilder.DropTable(
                name: "BranchManagerArchives");

            migrationBuilder.DropTable(
                name: "CashAddeds");

            migrationBuilder.DropTable(
                name: "CashExpenses");

            migrationBuilder.DropTable(
                name: "CommissaryArchives");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "ItemTranArchives");

            migrationBuilder.DropTable(
                name: "ItemTransactions");

            migrationBuilder.DropTable(
                name: "OrderComments");

            migrationBuilder.DropTable(
                name: "OrderListArchives");

            migrationBuilder.DropTable(
                name: "OrderLists");

            migrationBuilder.DropTable(
                name: "ResItemsArchives");

            migrationBuilder.DropTable(
                name: "RestaurantArchives");

            migrationBuilder.DropTable(
                name: "RestaurantInventories");

            migrationBuilder.DropTable(
                name: "StaffArchives");

            migrationBuilder.DropTable(
                name: "StaffTimeSlots");

            migrationBuilder.DropTable(
                name: "STimeArchives");

            migrationBuilder.DropTable(
                name: "SubBranchOrderArchives");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.DropTable(
                name: "SupplyHistories");

            migrationBuilder.DropTable(
                name: "SupplyHistoryArchives");

            migrationBuilder.DropTable(
                name: "SupplyListArchives");

            migrationBuilder.DropTable(
                name: "SupplyLists");

            migrationBuilder.DropTable(
                name: "SupplyOrderArchives");

            migrationBuilder.DropTable(
                name: "TransactionArchives");

            migrationBuilder.DropTable(
                name: "CashRegisters");

            migrationBuilder.DropTable(
                name: "ExpenseTypes");

            migrationBuilder.DropTable(
                name: "RestaurantItems");

            migrationBuilder.DropTable(
                name: "RestaurantTransactions");

            migrationBuilder.DropTable(
                name: "OrderInfos");

            migrationBuilder.DropTable(
                name: "CommissaryInventories");

            migrationBuilder.DropTable(
                name: "SupplyOrders");

            migrationBuilder.DropTable(
                name: "ItemCategories");

            migrationBuilder.DropTable(
                name: "StaffInformations");

            migrationBuilder.DropTable(
                name: "TransactionTypes");

            migrationBuilder.DropTable(
                name: "BranchManagers");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "VendorInfos");

            migrationBuilder.DropTable(
                name: "BranchLocations");
        }
    }
}
