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

        public int? SubCategoryId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal CostPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal SellingPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Stock { get; set; }

        public string Yield { get; set; }

        [MaxLength(50)]
        public string Uom { get; set; }

        public int? SkuId { get; set; }

        public bool IsRepack { get; set; }

        public int? VendorId { get; set; }
    }
}
