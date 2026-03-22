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

        [Column(TypeName = "decimal(18,4)")]
        public decimal QuantityChange { get; set; }

        [MaxLength(50)]
        public string? Uom { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? TotalPrice { get; set; }

        public bool IsPaid { get; set; }

        public string? Remarks { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}

