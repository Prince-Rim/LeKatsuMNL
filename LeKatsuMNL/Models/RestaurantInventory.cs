using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class RestaurantInventory
    {
        [Key]
        public int ResId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BeginningStock { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AddedStock { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DeductedStock { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentStock { get; set; }

        public int StaffId { get; set; }
        public StaffInformation StaffInputted { get; set; }
    }
}
