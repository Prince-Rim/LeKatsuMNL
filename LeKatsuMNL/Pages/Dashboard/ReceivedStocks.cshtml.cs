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

        public decimal UnpaidTotal { get; set; }
        public int UnpaidCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FilterStatus { get; set; }

        public class ReceivedStockItemInput
        {
            public int ComId { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public string Unit { get; set; }
        }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync(int? pageIndex)
        {
            if (string.IsNullOrEmpty(FilterStatus)) FilterStatus = "All";
            
            Vendors = await _context.VendorInfos.Where(v => !v.IsArchived).OrderBy(v => v.VendorName).ToListAsync();
            AllItems = await _context.CommissaryInventories
                .AsNoTracking()
                .Where(i => i.SkuId == null && !i.IsArchived)
                .OrderBy(i => i.ItemName).ToListAsync();

            // Summaries for Unpaid orders
            var unpaidOrders = await _context.SupplyOrders
                .Where(so => !so.IsArchived && so.PaymentStatus == "Unpaid")
                .Include(so => so.SupplyLists)
                .ToListAsync();

            UnpaidCount = unpaidOrders.Count;
            UnpaidTotal = unpaidOrders.Sum(so => so.SupplyLists.Sum(sl => sl.TotalPrice));

            var ordersQuery = _context.SupplyOrders
                .Where(so => !so.IsArchived)
                .Include(so => so.Vendor) // Fixed: Added Vendor include
                .Include(so => so.SupplyLists)
                    .ThenInclude(sl => sl.CommissaryInventory)
                .AsQueryable();

            if (!string.IsNullOrEmpty(FilterStatus) && FilterStatus != "All")
            {
                ordersQuery = ordersQuery.Where(so => so.PaymentStatus == FilterStatus);
            }

            var orders = ordersQuery.OrderByDescending(so => so.SupplyDate);

            int pageSize = PageSize > 0 ? PageSize : 10;
            SupplyOrders = await PaginatedList<SupplyOrder>.CreateAsync(orders.AsNoTracking(), pageIndex ?? 1, pageSize);
        }

        public async Task<IActionResult> OnPostSaveAsync(string PaymentStatus)
        {
            if (SelectedVendorId == 0 || ItemsToReceive == null || !ItemsToReceive.Any())
            {
                return RedirectToPage();
            }

            var supplyOrder = new SupplyOrder
            {
                SupplyDate = DateTime.Now,
                Status = "Received",
                PaymentStatus = PaymentStatus ?? "Unpaid",
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
                    decimal lineTotal = item.Quantity * item.UnitPrice;
                    decimal unitCostPerUom = inventoryItem.CostPrice;

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
                                    decimal conversionFactor = UomConverter.Convert(yieldSize, yieldUom, inventoryItem.Uom);
                                    actualQuantityAdded = item.Quantity * conversionFactor;
                                    
                                    if (conversionFactor > 0)
                                    {
                                        unitCostPerUom = item.UnitPrice / conversionFactor;
                                    }
                                }
                            }
                        }
                        catch { } // fallback to original item.Quantity if parsing fails
                    }
                    else if (item.Unit == inventoryItem.Uom)
                    {
                        unitCostPerUom = item.UnitPrice;
                    }

                    // Create Supply List entry
                    var supplyList = new SupplyList
                    {
                        SupplyId = supplyOrder.SoaId,
                        ComId = item.ComId,
                        Quantity = actualQuantityAdded,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = lineTotal
                    };
                    _context.SupplyLists.Add(supplyList);

                    // Update Inventory
                    inventoryItem.CostPrice = unitCostPerUom;
                    inventoryItem.Stock += actualQuantityAdded;

                    // Record Transaction
                    var transaction = new InventoryTransaction
                    {
                        ComId = item.ComId,
                        TypeId = transactionType.TypeId,
                        QuantityChange = actualQuantityAdded,
                        UnitPrice = unitCostPerUom,
                        TotalPrice = lineTotal,
                        IsPaid = (PaymentStatus == "Paid"),
                        TimeStamp = DateTime.Now,
                        Remarks = $"Stock In via Supply Order #{supplyOrder.SoaId:D5}"
                    };

                    _context.InventoryTransactions.Add(transaction);
                }
            }

            await _context.SaveChangesAsync();
            StatusMessage = "Successfully recorded stock receipt and updated inventory.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkAsPaidAsync(int id)
        {
            var order = await _context.SupplyOrders
                .FirstOrDefaultAsync(so => so.SoaId == id);
            
            if (order != null)
            {
                order.PaymentStatus = "Paid";
                await _context.SaveChangesAsync();
                StatusMessage = $"Order SO-{id:D5} marked as Paid.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBulkMarkAsPaidAsync([FromForm] List<int> selectedIds)
        {
            if (selectedIds != null && selectedIds.Any())
            {
                var orders = await _context.SupplyOrders
                    .Where(so => selectedIds.Contains(so.SoaId) && so.PaymentStatus == "Unpaid")
                    .ToListAsync();

                foreach (var order in orders)
                {
                    order.PaymentStatus = "Paid";
                }

                await _context.SaveChangesAsync();
                StatusMessage = $"Updated {orders.Count} orders to Paid.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostArchiveAsync(int id)
        {
            var supplyOrder = await _context.SupplyOrders.FindAsync(id);
            if (supplyOrder != null)
            {
                supplyOrder.IsArchived = true;
                await _context.SaveChangesAsync();
                StatusMessage = "Supply Order archived successfully.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBulkArchiveAsync([FromForm] List<int> selectedIds)
        {
            if (selectedIds != null && selectedIds.Any())
            {
                var orders = await _context.SupplyOrders
                    .Where(so => selectedIds.Contains(so.SoaId))
                    .ToListAsync();

                foreach (var order in orders)
                {
                    order.IsArchived = true;
                }

                await _context.SaveChangesAsync();
                StatusMessage = $"Archived {orders.Count} supply orders.";
            }
            return RedirectToPage();
        }
    }
}
