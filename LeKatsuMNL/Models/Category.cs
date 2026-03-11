using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property for One-to-Many
        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public ICollection<CommissaryInventory> CommissaryInventories { get; set; } = new List<CommissaryInventory>();
        public ICollection<RestaurantInventory> RestaurantInventories { get; set; } = new List<RestaurantInventory>();
    }
}
