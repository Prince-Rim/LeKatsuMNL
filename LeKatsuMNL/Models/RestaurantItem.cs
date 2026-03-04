using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class RestaurantItem
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; }

        public int ItemCtgId { get; set; }
        public ItemCategory ItemCategory { get; set; }

        public string ItemImg { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        [MaxLength(100)]
        public string Modifier { get; set; }

        public bool Availability { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool VatExempt { get; set; }

        // Navigation Properties
        public ICollection<ItemTransaction> ItemTransactions { get; set; } = new List<ItemTransaction>();
    }
}
