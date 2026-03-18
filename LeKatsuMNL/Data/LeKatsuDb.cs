using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Models;

namespace LeKatsuMNL.Data
{
    public class LeKatsuDb : DbContext
    {
        public LeKatsuDb(DbContextOptions<LeKatsuDb> options) : base(options) { }

        // Master Data & Configuration
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<InvTransactionType> InvTransactionTypes { get; set; }
        public DbSet<ResTransactionType> ResTransactionTypes { get; set; }
        public DbSet<ExpenseType> ExpenseTypes { get; set; }

        // Users & Staff
        public DbSet<BranchManager> BranchManagers { get; set; }
        public DbSet<AdminAccount> AdminAccounts { get; set; }
        public DbSet<BranchLocation> BranchLocations { get; set; }
        public DbSet<StaffInformation> StaffInformations { get; set; }
        public DbSet<StaffTimeSlot> StaffTimeSlots { get; set; }

        // Commissary Inventory
        public DbSet<VendorInfo> VendorInfos { get; set; }
        public DbSet<CommissaryInventory> CommissaryInventories { get; set; }
        
        // Supply Chain
        public DbSet<SupplyOrder> SupplyOrders { get; set; }
        public DbSet<SupplyList> SupplyLists { get; set; }
        public DbSet<SupplyHistory> SupplyHistories { get; set; }

        // Orders System
        public DbSet<OrderInfo> OrderInfos { get; set; }
        public DbSet<OrderList> OrderLists { get; set; }
        public DbSet<OrderComment> OrderComments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        // SKU & Recipes
        public DbSet<SkuHeader> SkuHeaders { get; set; }
        public DbSet<SkuRecipe> SkuRecipes { get; set; }
        public DbSet<IngredientRecipe> IngredientRecipes { get; set; }

        // Restaurant Inventory & Sales
        public DbSet<RestaurantItem> RestaurantItems { get; set; }
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<RestaurantInventory> RestaurantInventories { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<RestaurantTransaction> RestaurantTransactions { get; set; }
        public DbSet<ItemTransaction> ItemTransactions { get; set; }

        // Cash Register
        public DbSet<CashRegister> CashRegisters { get; set; }
        public DbSet<CashAdded> CashAddeds { get; set; }
        public DbSet<CashExpense> CashExpenses { get; set; }

        // Archives
        public DbSet<CommissaryArchive> CommissaryArchives { get; set; }
        public DbSet<SupplyOrderArchive> SupplyOrderArchives { get; set; }
        public DbSet<SupplyListArchive> SupplyListArchives { get; set; }
        public DbSet<SupplyHistoryArchive> SupplyHistoryArchives { get; set; }
        public DbSet<SubBranchOrderArchive> SubBranchOrderArchives { get; set; }
        public DbSet<OrderListArchive> OrderListArchives { get; set; }
        public DbSet<BranchManagerArchive> BranchManagerArchives { get; set; }
        public DbSet<AdminArchive> AdminArchives { get; set; }
        public DbSet<BranchLocationArchive> BranchLocationArchives { get; set; }
        public DbSet<RestaurantArchive> RestaurantArchives { get; set; }
        public DbSet<ResItemsArchive> ResItemsArchives { get; set; }
        public DbSet<TransactionArchive> TransactionArchives { get; set; }
        public DbSet<ItemTranArchive> ItemTranArchives { get; set; }
        public DbSet<StaffArchive> StaffArchives { get; set; }
        public DbSet<STimeArchive> STimeArchives { get; set; }
        public DbSet<RejectItem> RejectItems { get; set; }
        public DbSet<SkuArchive> SkuArchives { get; set; }
        public DbSet<VendorArchive> VendorArchives { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and cascading deletes to prevent cycles if needed

            // Category -> SubCategory (1:M)
            modelBuilder.Entity<SubCategory>()
                .HasOne(sc => sc.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // VendorInfo -> CommissaryInventory (1:M)
            modelBuilder.Entity<CommissaryInventory>()
                .HasOne(ci => ci.Vendor)
                .WithMany(v => v.CommissaryInventories)
                .HasForeignKey(ci => ci.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category -> CommissaryInventory (1:M)
            modelBuilder.Entity<CommissaryInventory>()
                .HasOne(ci => ci.Category)
                .WithMany(c => c.CommissaryInventories)
                .HasForeignKey(ci => ci.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubCategory -> CommissaryInventory (1:M)
            modelBuilder.Entity<CommissaryInventory>()
                .HasOne(ci => ci.SubCategory)
                .WithMany()
                .HasForeignKey(ci => ci.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // SkuHeader -> SubCategory (1:M)
            modelBuilder.Entity<SkuHeader>()
                .HasOne(s => s.SubCategory)
                .WithMany()
                .HasForeignKey(s => s.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // SkuHeader -> CommissaryInventory (1:M)
            modelBuilder.Entity<CommissaryInventory>()
                .HasOne(ci => ci.SkuHeader)
                .WithMany() // SkuHeader doesn't have CommissaryInventories collection
                .HasForeignKey(ci => ci.SkuId)
                .OnDelete(DeleteBehavior.Restrict);

            // CommissaryInventory -> InventoryTransaction (1:M)
            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(it => it.CommissaryInventory)
                .WithMany(ci => ci.InventoryTransactions)
                .HasForeignKey(it => it.ComId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderInfo -> OrderList (1:M)
            modelBuilder.Entity<OrderList>()
                .HasOne(ol => ol.OrderInfo)
                .WithMany(o => o.OrderLists)
                .HasForeignKey(ol => ol.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // SkuHeader -> OrderList (1:M)
            modelBuilder.Entity<OrderList>()
                .HasOne(ol => ol.SkuHeader)
                .WithMany(s => s.OrderLists)
                .HasForeignKey(ol => ol.SkuId)
                .OnDelete(DeleteBehavior.Restrict);

            // CommissaryInventory -> OrderList (1:M)
            modelBuilder.Entity<OrderList>()
                .HasOne(ol => ol.CommissaryInventory)
                .WithMany(ci => ci.OrderLists)
                .HasForeignKey(ol => ol.ComId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderInfo -> OrderComment (1:M)
            modelBuilder.Entity<OrderComment>()
                .HasOne(oc => oc.OrderInfo)
                .WithMany(o => o.OrderComments)
                .HasForeignKey(oc => oc.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // BranchManager -> OrderComment (1:M)
            modelBuilder.Entity<OrderComment>()
                .HasOne(oc => oc.BranchManager)
                .WithMany(bm => bm.OrderComments)
                .HasForeignKey(oc => oc.BranchManagerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // AdminAccount -> OrderComment (1:M)
            modelBuilder.Entity<OrderComment>()
                .HasOne(oc => oc.AdminAccount)
                .WithMany() // AdminAccount doesn't have OrderComments collection yet, but we can add it later if needed
                .HasForeignKey(oc => oc.AdminAccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderInfo -> Invoice (1:M)
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.OrderInfo)
                .WithMany(o => o.Invoices)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // BranchLocation -> BranchManager (1:M)
            modelBuilder.Entity<BranchManager>()
                .HasOne(bm => bm.BranchLocation)
                .WithMany(bl => bl.BranchManagers)
                .HasForeignKey(bm => bm.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // BranchManager -> OrderInfo (1:M)
            modelBuilder.Entity<OrderInfo>()
                .HasOne(oi => oi.BranchManager)
                .WithMany(bm => bm.Orders)
                .HasForeignKey(oi => oi.BranchManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // StaffInformation -> StaffTimeSlot (1:M)
            modelBuilder.Entity<StaffTimeSlot>()
                .HasOne(sts => sts.Staff)
                .WithMany(si => si.StaffTimeSlots)
                .HasForeignKey(sts => sts.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            // ItemCategory -> RestaurantItem (1:M)
            modelBuilder.Entity<RestaurantItem>()
                .HasOne(ri => ri.ItemCategory)
                .WithMany(ic => ic.RestaurantItems)
                .HasForeignKey(ri => ri.ItemCtgId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category -> RestaurantInventory (1:M)
            modelBuilder.Entity<RestaurantInventory>()
                .HasOne(ri => ri.Category)
                .WithMany(c => c.RestaurantInventories)
                .HasForeignKey(ri => ri.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // StaffInformation -> RestaurantInventory (1:M)
            modelBuilder.Entity<RestaurantInventory>()
                .HasOne(ri => ri.StaffInputted)
                .WithMany(si => si.RestaurantInventories)
                .HasForeignKey(ri => ri.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            // InvTransactionType -> InventoryTransaction (1:M)
            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(it => it.InvTransactionType)
                .WithMany(itt => itt.InventoryTransactions)
                .HasForeignKey(it => it.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ResTransactionType -> RestaurantTransaction (1:M)
            modelBuilder.Entity<RestaurantTransaction>()
                .HasOne(rt => rt.ResTransactionType)
                .WithMany(rtt => rtt.RestaurantTransactions)
                .HasForeignKey(rt => rt.TtId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // StaffInformation -> RestaurantTransaction (1:M)
            modelBuilder.Entity<RestaurantTransaction>()
                .HasOne(rt => rt.Staff)
                .WithMany(si => si.RestaurantTransactions)
                .HasForeignKey(rt => rt.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            // RestaurantTransaction -> ItemTransaction (1:M)
            modelBuilder.Entity<ItemTransaction>()
                .HasOne(it => it.RestaurantTransaction)
                .WithMany(rt => rt.ItemTransactions)
                .HasForeignKey(it => it.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            // RestaurantItem -> ItemTransaction (1:M)
            modelBuilder.Entity<ItemTransaction>()
                .HasOne(it => it.RestaurantItem)
                .WithMany(ri => ri.ItemTransactions)
                .HasForeignKey(it => it.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // SupplyOrder -> SupplyList (1:M)
            modelBuilder.Entity<SupplyList>()
                .HasOne(sl => sl.SupplyOrder)
                .WithMany(so => so.SupplyLists)
                .HasForeignKey(sl => sl.SupplyId)
                .OnDelete(DeleteBehavior.Cascade);

            // CommissaryInventory -> SupplyList (1:M)
            modelBuilder.Entity<SupplyList>()
                .HasOne(sl => sl.CommissaryInventory)
                .WithMany(ci => ci.SupplyLists)
                .HasForeignKey(sl => sl.ComId)
                .OnDelete(DeleteBehavior.Restrict);

            // SupplyOrder -> SupplyHistory (1:M)
            modelBuilder.Entity<SupplyHistory>()
                .HasOne(sh => sh.SupplyOrder)
                .WithMany(so => so.SupplyHistories)
                .HasForeignKey(sh => sh.SupplyId)
                .OnDelete(DeleteBehavior.Cascade);

            // CashRegister -> CashAdded (1:M)
            modelBuilder.Entity<CashAdded>()
                .HasOne(ca => ca.CashRegister)
                .WithMany(cr => cr.CashAddeds)
                .HasForeignKey(ca => ca.CrId)
                .OnDelete(DeleteBehavior.Cascade);

            // CashRegister -> CashExpense (1:M)
            modelBuilder.Entity<CashExpense>()
                .HasOne(ce => ce.CashRegister)
                .WithMany(cr => cr.CashExpensesList) // Handled naming collision in the model
                .HasForeignKey(ce => ce.CrId)
                .OnDelete(DeleteBehavior.Cascade);

            // ExpenseType -> CashExpense (1:M)
            modelBuilder.Entity<CashExpense>()
                .HasOne(ce => ce.ExpenseType)
                .WithMany(et => et.CashExpenses)
                .HasForeignKey(ce => ce.ExpenseId)
                .OnDelete(DeleteBehavior.Restrict);

            // SkuHeader -> SkuRecipe (1:M)
            modelBuilder.Entity<SkuRecipe>()
                .HasOne(sr => sr.SkuHeader)
                .WithMany(sh => sh.SkuRecipes)
                .HasForeignKey(sr => sr.SkuId)
                .OnDelete(DeleteBehavior.Cascade);

            // CommissaryInventory -> SkuRecipe (1:M)
            modelBuilder.Entity<SkuRecipe>()
                .HasOne(sr => sr.CommissaryInventory)
                .WithMany(ci => ci.SkuRecipes)
                .HasForeignKey(sr => sr.ComId)
                .OnDelete(DeleteBehavior.Restrict);

            // SkuHeader (Nested) -> SkuRecipe (1:M)
            modelBuilder.Entity<SkuRecipe>()
                .HasOne(sr => sr.TargetSku)
                .WithMany()
                .HasForeignKey(sr => sr.TargetSkuId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ingredient Details (Repacking)
            modelBuilder.Entity<IngredientRecipe>()
                .HasOne(ir => ir.Parent)
                .WithMany(ci => ci.IngredientRecipes)
                .HasForeignKey(ir => ir.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IngredientRecipe>()
                .HasOne(ir => ir.Material)
                .WithMany()
                .HasForeignKey(ir => ir.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
