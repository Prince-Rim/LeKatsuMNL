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

        public int? SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }


        [Column(TypeName = "decimal(18,4)")]
        public decimal CostPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal SellingPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Stock { get; set; }

        public string Yield { get; set; }

        [Required]
        [MaxLength(50)]
        public string Uom { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? ReorderValue { get; set; }

        public int? SkuId { get; set; }
        public SkuHeader SkuHeader { get; set; }

        public bool IsRepack { get; set; }

        public int? PriceId { get; set; }

        public int? VendorId { get; set; }
        public VendorInfo Vendor { get; set; }
        
        // Navigation Properties
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public ICollection<OrderList> OrderLists { get; set; } = new List<OrderList>();
        public ICollection<SupplyList> SupplyLists { get; set; } = new List<SupplyList>();
        public ICollection<SkuRecipe> SkuRecipes { get; set; } = new List<SkuRecipe>();
        public ICollection<IngredientRecipe> IngredientRecipes { get; set; } = new List<IngredientRecipe>();

    }
}
