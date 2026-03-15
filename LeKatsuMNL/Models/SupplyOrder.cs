using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class SupplyOrder
    {
        [Key]
        public int SoaId { get; set; }

        public DateTime SupplyDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        public string? ReceiptImg { get; set; }

        public int? VendorId { get; set; }
        public VendorInfo? Vendor { get; set; }

        // Navigation Properties
        public ICollection<SupplyList> SupplyLists { get; set; } = new List<SupplyList>();
        public ICollection<SupplyHistory> SupplyHistories { get; set; } = new List<SupplyHistory>();
    }
}
