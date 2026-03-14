using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class SkuHeader
    {
        [Key]
        public int SkuId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; }

        [MaxLength(100)]
        public string? SubCategory { get; set; }

        [MaxLength(100)]
        public string SubClass { get; set; }

        [MaxLength(50)]
        public string PackagingType { get; set; }

        [MaxLength(50)]
        public string PackagingUnit { get; set; }

        [MaxLength(50)]
        public string PackSize { get; set; }

        [MaxLength(50)]
        public string Uom { get; set; }

        [MaxLength(150)]
        public string Supplier { get; set; }

        public bool IsSellingPriceEnabled { get; set; }
        public bool IsReorderLevelEnabled { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? SellingPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitCost { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Navigation Property: Link to the recipes
        public ICollection<SkuRecipe> SkuRecipes { get; set; } = new List<SkuRecipe>();
        
        // Navigation Property: Link to the Order Lists
        public ICollection<OrderList> OrderLists { get; set; } = new List<OrderList>();
    }
}
