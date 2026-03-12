using System;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class RejectItem
    {
        [Key]
        public int RejectId { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public string Uom { get; set; } // Replaces Type
        public string Reason { get; set; }
        public DateTime RejectedAt { get; set; }
        
        // Distinguishes between "Recipe" (Item) and "SKU"
        public string RejectType { get; set; } = "Recipe"; 
    }
}
