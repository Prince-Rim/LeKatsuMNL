using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class SupplyOrderArchive
    {
        [Key]
        public int SoaId { get; set; }

        public int SListId { get; set; }

        public DateTime SupplyDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        public string ReceiptImg { get; set; }

        public int? VendorId { get; set; }
        public VendorInfo Vendor { get; set; }
    }
}
