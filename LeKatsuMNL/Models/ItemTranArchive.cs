using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class ItemTranArchive
    {
        [Key]
        public int ItaId { get; set; }

        public int ItemTranId { get; set; }

        public int TransactionId { get; set; }

        public int ItemId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }
    }
}
