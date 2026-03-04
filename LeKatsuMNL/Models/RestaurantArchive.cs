using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class RestaurantArchive
    {
        [Key]
        public int RaId { get; set; }

        public int ResId { get; set; }

        [MaxLength(100)]
        public string ItemName { get; set; }

        public int CategoryId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BeginningStock { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AddedStock { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DeductedStock { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentStock { get; set; }

        public int StaffInputted { get; set; }
    }
}
