using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        
        // Optional links to Item or SKU
        public int? ComId { get; set; }
        public int? SkuId { get; set; }

        // Navigation properties (optional)
        [ForeignKey("ComId")]
        public CommissaryInventory? CommissaryInventory { get; set; }
        [ForeignKey("SkuId")]
        public SkuHeader? SkuHeader { get; set; }

        // Distinguishes between "Recipe" (Item) and "SKU"
        public string RejectType { get; set; } = "Recipe"; 
    }
}
