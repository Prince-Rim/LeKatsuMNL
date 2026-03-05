using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class SkuHeader
    {
        [Key]
        public int SkuId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Navigation Property: Link to the recipes
        public ICollection<SkuRecipe> SkuRecipes { get; set; } = new List<SkuRecipe>();
        
        // Navigation Property: Link to the Order Lists
        public ICollection<OrderList> OrderLists { get; set; } = new List<OrderList>();
    }
}
