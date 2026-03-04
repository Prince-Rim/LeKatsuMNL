using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class CashAdded
    {
        [Key]
        public int CshAddId { get; set; }

        public int CrId { get; set; }
        public CashRegister CashRegister { get; set; }

        public DateTime DateTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AddedAmount { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }
    }
}
