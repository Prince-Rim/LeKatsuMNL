using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class VendorInfo
    {
        [Key]
        public int VendorId { get; set; }

        [Required]
        [MaxLength(150)]
        public string VendorName { get; set; }

        [MaxLength(20)]
        public string ContactNum { get; set; }

        [MaxLength(150)]
        public string SecondVendorName { get; set; }

        [MaxLength(20)]
        public string SecondVendorCn { get; set; }

        [MaxLength(20)]
        public string SupplierType { get; set; } = "Main";
        
        [MaxLength(50)]
        public string? CompanyId { get; set; }

        [MaxLength(150)]
        public string? CompanyName { get; set; }

        [MaxLength(255)]
        public string? CompanyAddress { get; set; }

        [MaxLength(20)]
        public string? Tin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property for One-to-Many
        public ICollection<CommissaryInventory> CommissaryInventories { get; set; } = new List<CommissaryInventory>();
    }
}
