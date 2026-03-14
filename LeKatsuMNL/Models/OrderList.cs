using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class OrderList
    {
        [Key]
        public int ListId { get; set; }

        public int OrderId { get; set; }
        public OrderInfo OrderInfo { get; set; }

        public int SkuId { get; set; }
        public SkuHeader SkuHeader { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalPrice { get; set; }
    }
}
