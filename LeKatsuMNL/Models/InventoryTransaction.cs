using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        public int ComId { get; set; }
        public CommissaryInventory CommissaryInventory { get; set; }

        public int TypeId { get; set; }
        public InvTransactionType InvTransactionType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityChange { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
