using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class CommissaryInventory
    {
        [Key]
        public int ComId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Stock { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Yield { get; set; }

        [Required]
        [MaxLength(50)]
        public string Uom { get; set; } // Unit of Measure

        public int VendorId { get; set; }
        public VendorInfo Vendor { get; set; }
        
        // Navigation Properties
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public ICollection<OrderList> OrderLists { get; set; } = new List<OrderList>();
        public ICollection<SupplyList> SupplyLists { get; set; } = new List<SupplyList>();
    }
}
