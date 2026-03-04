using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class SupplyList
    {
        [Key]
        public int SListId { get; set; }

        public int SupplyId { get; set; }
        public SupplyOrder SupplyOrder { get; set; }

        public int ComId { get; set; }
        public CommissaryInventory CommissaryInventory { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
    }
}
