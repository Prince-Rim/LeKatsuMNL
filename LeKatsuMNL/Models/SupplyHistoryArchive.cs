using System;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class SupplyHistoryArchive
    {
        [Key]
        public int ShaId { get; set; }

        public int SHistoryId { get; set; }

        public int SupplyId { get; set; }

        public int SListId { get; set; }

        public DateTime SupplyDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        public string ReceiptImg { get; set; }
    }
}
