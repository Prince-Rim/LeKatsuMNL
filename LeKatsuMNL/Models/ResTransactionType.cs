using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class ResTransactionType
    {
        [Key]
        public int TtId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TransactionType { get; set; } // e.g Cash, GCash, Bank Transfer

        // Navigation Properties
        public ICollection<RestaurantTransaction> RestaurantTransactions { get; set; } = new List<RestaurantTransaction>();
    }
}
