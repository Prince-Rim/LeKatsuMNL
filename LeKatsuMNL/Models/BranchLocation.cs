using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class BranchLocation
    {
        [Key]
        public int BranchId { get; set; }

        [Required]
        [MaxLength(150)]
        public string BranchName { get; set; }

        // Compiled full address string (auto-generated from structured fields)
        [Required]
        [MaxLength(500)]
        public string BranchLocationAddress { get; set; }

        // Structured address fields (PSGC-based)
        [Required]
        [MaxLength(50)]
        public string IslandGroup { get; set; }

        [Required]
        [MaxLength(100)]
        public string Region { get; set; }

        [MaxLength(100)]
        public string? Province { get; set; }

        [Required]
        [MaxLength(150)]
        public string CityMunicipality { get; set; }

        [Required]
        [MaxLength(150)]
        public string Barangay { get; set; }

        [MaxLength(255)]
        public string? StreetAddress { get; set; }

        [MaxLength(10)]
        public string? ZipCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public ICollection<BranchManager> BranchManagers { get; set; } = new List<BranchManager>();
    }
}
