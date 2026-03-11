using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class CommissaryArchive
    {
        [Key]
        public int CaId { get; set; }

        public int ComId { get; set; }

        [MaxLength(100)]
        public string ItemName { get; set; }

        public int CategoryId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Stock { get; set; }

        public string Yield { get; set; }

        [MaxLength(50)]
        public string Uom { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ReorderValue { get; set; }

        public int? SkuId { get; set; }
        public SkuHeader SkuHeader { get; set; }

        public int? PriceId { get; set; }

        public int VendorId { get; set; }
    }
}
