using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class ItemCategory
    {
        [Key]
        public int ItemCtgId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ItemCategoryName { get; set; }

        // Navigation Properties
        public ICollection<RestaurantItem> RestaurantItems { get; set; } = new List<RestaurantItem>();
    }
}
