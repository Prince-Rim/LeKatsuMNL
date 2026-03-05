using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class SkuRecipe
    {
        [Key]
        public int RecipeId { get; set; }

        // Foreign Key to SkuHeader
        public int SkuId { get; set; }
        public SkuHeader SkuHeader { get; set; }

        // Foreign Key to CommissaryInventory
        public int ComId { get; set; }
        public CommissaryInventory CommissaryInventory { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityNeeded { get; set; }

        [Required]
        [MaxLength(50)]
        public string Uom { get; set; }
    }
}
