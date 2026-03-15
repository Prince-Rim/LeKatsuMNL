using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class SkuRecipe
    {
        [Key]
        public int RecipeId { get; set; }

        // Foreign Key to SkuHeader (Parent SKU)
        public int SkuId { get; set; }
        public SkuHeader SkuHeader { get; set; }

        // Foreign Key to CommissaryInventory (Optional if TargetSkuId is used)
        public int? ComId { get; set; }
        public CommissaryInventory? CommissaryInventory { get; set; }

        // Foreign Key to SkuHeader (Nested SKU)
        public int? TargetSkuId { get; set; }
        public SkuHeader? TargetSku { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal QuantityNeeded { get; set; }

        [Required]
        [MaxLength(50)]
        public string Uom { get; set; }
    }
}
