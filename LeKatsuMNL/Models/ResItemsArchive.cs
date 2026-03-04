using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class ResItemsArchive
    {
        [Key]
        public int RiaId { get; set; }

        public int ItemId { get; set; }

        [MaxLength(100)]
        public string ItemName { get; set; }

        public int ItemCategory { get; set; }

        public string ItemImg { get; set; }

        [MaxLength(100)]
        public string Option { get; set; } // Based on the ERD (Might correspond to Modifier)

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        [MaxLength(100)]
        public string Modifier { get; set; }

        public bool Availability { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool VatExempt { get; set; }
    }
}
