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

        [Column(TypeName = "decimal(18,2)")]
        public decimal Yield { get; set; }

        [MaxLength(50)]
        public string Uom { get; set; }

        public int VendorId { get; set; }
    }
}
