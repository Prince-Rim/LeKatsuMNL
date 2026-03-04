using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class TransactionArchive
    {
        [Key]
        public int RtaId { get; set; }

        public int TransactionId { get; set; }

        public DateTime DateTime { get; set; }

        [MaxLength(50)]
        public string ReceiptNum { get; set; }

        public int StaffId { get; set; }

        public int ItemTranId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public bool IsRefunded { get; set; }
    }
}
