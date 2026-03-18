using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class SkuArchive
    {
        [Key]
        public int SaId { get; set; }
        
        public int SkuId { get; set; }
        
        [MaxLength(100)]
        public string ItemName { get; set; }
        
        public int? SubCategoryId { get; set; }
        
        public int CategoryId { get; set; }
        
        [MaxLength(50)]
        public string PackagingType { get; set; }
        
        [MaxLength(50)]
        public string PackagingUnit { get; set; }
        
        [MaxLength(50)]
        public string PackSize { get; set; }
        
        [MaxLength(50)]
        public string Uom { get; set; }
        
        public bool IsSellingPriceEnabled { get; set; }
        
        public bool IsReorderLevelEnabled { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal SellingPrice { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitCost { get; set; }
    }
}
