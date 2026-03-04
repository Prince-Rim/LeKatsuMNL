using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class RestaurantTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        public int TtId { get; set; }
        public ResTransactionType ResTransactionType { get; set; }

        public DateTime DateTime { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReceiptNum { get; set; }

        public int StaffId { get; set; }
        public StaffInformation Staff { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public bool IsRefunded { get; set; }

        // Navigation Properties (One-to-Many to ItemTransactions)
        public ICollection<ItemTransaction> ItemTransactions { get; set; } = new List<ItemTransaction>();
    }
}
