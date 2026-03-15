using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LeKatsuMNL.Helpers;
using LeKatsuMNL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class ReceivedStocksModel : PageModel
    {
        private readonly LeKatsuMNL.Data.LeKatsuDb _context;

        public ReceivedStocksModel(LeKatsuMNL.Data.LeKatsuDb context)
        {
            _context = context;
        }

        public class ReceivedStockRow
        {
            public int ComId { get; set; }
            public string ItemName { get; set; }
            public decimal Quantity { get; set; }
            public string Unit { get; set; }
            public string ReceivedDate { get; set; }
        }

        public List<VendorInfo> Vendors { get; set; }
        public List<CommissaryInventory> AllItems { get; set; }
        public PaginatedList<SupplyOrder> SupplyOrders { get; set; }

        [BindProperty]
        public int SelectedVendorId { get; set; }

        [BindProperty]
        public List<ReceivedStockItemInput> ItemsToReceive { get; set; }

        public class ReceivedStockItemInput
        {
            public int ComId { get; set; }
            public decimal Quantity { get; set; }
            public string Unit { get; set; }
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync(int? pageIndex)
        {
            Vendors = await _context.VendorInfos.OrderBy(v => v.VendorName).ToListAsync();
            AllItems = await _context.CommissaryInventories.OrderBy(i => i.ItemName).ToListAsync();

            var orders = _context.SupplyOrders
                .Include(so => so.SupplyLists)
                    .ThenInclude(sl => sl.CommissaryInventory)
                .OrderByDescending(so => so.SupplyDate);

            SupplyOrders = await PaginatedList<SupplyOrder>.CreateAsync(orders.AsNoTracking(), pageIndex ?? 1, 10);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (SelectedVendorId == 0 || ItemsToReceive == null || !ItemsToReceive.Any())
            {
                return RedirectToPage();
            }

            var supplyOrder = new SupplyOrder
            {
                SupplyDate = DateTime.Now,
                Status = "Received",
                DeliveryDate = DateTime.Now,
                VendorId = SelectedVendorId
            };

            _context.SupplyOrders.Add(supplyOrder);
            await _context.SaveChangesAsync();

            var transactionType = await _context.InvTransactionTypes.FirstOrDefaultAsync(t => t.TransactionType == "Stock In");
            
            if (transactionType == null)
            {
                transactionType = await _context.InvTransactionTypes.FirstOrDefaultAsync();
                if (transactionType == null)
                {
                    transactionType = new InvTransactionType { TransactionType = "Stock In" };
                    _context.InvTransactionTypes.Add(transactionType);
                    await _context.SaveChangesAsync();
                }
            }

            foreach (var item in ItemsToReceive.Where(i => i.ComId > 0 && i.Quantity > 0))
            {
                var inventoryItem = await _context.CommissaryInventories.FindAsync(item.ComId);
                if (inventoryItem != null)
                {
                    // Create Supply List entry
                    var supplyList = new SupplyList
                    {
                        SupplyId = supplyOrder.SoaId,
                        ComId = item.ComId,
                        Quantity = item.Quantity,
                        TotalPrice = 0 // Assuming price is handled elsewhere or set to 0 for now
                    };
                    _context.SupplyLists.Add(supplyList);

                    // Update Stock
                    inventoryItem.Stock += item.Quantity;

                    // Record Transaction
                    var transaction = new InventoryTransaction
                    {
                        ComId = item.ComId,
                        TypeId = transactionType.TypeId,
                        QuantityChange = item.Quantity,
                        TimeStamp = DateTime.Now
                    };
                    _context.InventoryTransactions.Add(transaction);
                }
            }

            await _context.SaveChangesAsync();
            StatusMessage = "Successfully recorded stock receipt and updated inventory.";
            return RedirectToPage();
        }
    }
}
