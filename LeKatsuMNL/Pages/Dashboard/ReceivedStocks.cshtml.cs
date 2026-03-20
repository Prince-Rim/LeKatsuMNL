using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LeKatsuMNL.Helpers;
using LeKatsuMNL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync(int? pageIndex)
        {
            Vendors = await _context.VendorInfos.Where(v => !v.IsArchived).OrderBy(v => v.VendorName).ToListAsync();
            AllItems = await _context.CommissaryInventories
                .AsNoTracking()
                .Where(i => i.SkuId == null && !i.IsArchived)
                .OrderBy(i => i.ItemName).ToListAsync();

            foreach (var item in AllItems)
            {
                item.Uom = UomConverter.NormalizeUnit(item.Uom);
            }

            var orders = _context.SupplyOrders
                .Where(so => !so.IsArchived)
                .Include(so => so.SupplyLists)
                    .ThenInclude(sl => sl.CommissaryInventory)
                .OrderByDescending(so => so.SupplyDate);

            int pageSize = PageSize > 0 ? PageSize : 10;
            SupplyOrders = await PaginatedList<SupplyOrder>.CreateAsync(orders.AsNoTracking(), pageIndex ?? 1, pageSize);
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
                    decimal actualQuantityAdded = item.Quantity;

                    // Parse Yield to convert if it matches the received unit
                    if (!string.IsNullOrEmpty(inventoryItem.Yield) && item.Unit == inventoryItem.Yield)
                    {
                        try 
                        {
                            var parts = inventoryItem.Yield.Split('/');
                            string sizeAndUnit = parts.Length == 2 ? parts[1].Trim() : parts[0].Trim();
                            var match = Regex.Match(sizeAndUnit, @"^([\d\.]+)\s*(.+)$");
                            if (match.Success)
                            {
                                if (decimal.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal yieldSize))
                                {
                                    string yieldUom = match.Groups[2].Value.Trim();
                                    actualQuantityAdded = item.Quantity * UomConverter.Convert(yieldSize, yieldUom, inventoryItem.Uom);
                                }
                            }
                        }
                        catch { } // fallback to original item.Quantity if parsing fails
                    }

                    // Create Supply List entry
                    var supplyList = new SupplyList
                    {
                        SupplyId = supplyOrder.SoaId,
                        ComId = item.ComId,
                        Quantity = actualQuantityAdded,
                        TotalPrice = 0 // Assuming price is handled elsewhere or set to 0 for now
                    };
                    _context.SupplyLists.Add(supplyList);

                    // Update Stock
                    inventoryItem.Stock += actualQuantityAdded;

                    // Record Transaction
                    var transaction = new InventoryTransaction
                    {
                        ComId = item.ComId,
                        TypeId = transactionType.TypeId,
                        QuantityChange = actualQuantityAdded,
                        TimeStamp = DateTime.Now
                    };
                    _context.InventoryTransactions.Add(transaction);
                }
            }

            await _context.SaveChangesAsync();
            StatusMessage = "Successfully recorded stock receipt and updated inventory.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostArchiveAsync(int id)
        {
            var supplyOrder = await _context.SupplyOrders.FindAsync(id);
            if (supplyOrder != null)
            {
                supplyOrder.IsArchived = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
