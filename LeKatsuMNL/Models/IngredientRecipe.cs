using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class IngredientRecipe
    {
        [Key]
        public int RecipeId { get; set; }

        // Foreign Key to CommissaryInventory (Parent Ingredient)
        public int ParentId { get; set; }
        public CommissaryInventory Parent { get; set; }

        // Foreign Key to CommissaryInventory (Material Ingredient)
        public int MaterialId { get; set; }
        public CommissaryInventory Material { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal QuantityNeeded { get; set; }

        [Required]
        [MaxLength(50)]
        public string Uom { get; set; }
    }
}
