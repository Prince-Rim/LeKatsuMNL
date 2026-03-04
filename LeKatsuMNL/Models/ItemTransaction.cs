using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class ItemTransaction
    {
        [Key]
        public int ItemTranId { get; set; }

        public int TransactionId { get; set; }
        public RestaurantTransaction RestaurantTransaction { get; set; }

        public int ItemId { get; set; }
        public RestaurantItem RestaurantItem { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }
    }
}
