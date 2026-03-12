using System;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class BranchLocationArchive
    {
        [Key]
        public int BlaId { get; set; }

        public int BranchId { get; set; }

        [MaxLength(150)]
        public string BranchName { get; set; }

        [MaxLength(500)]
        public string BranchLocationAddress { get; set; }

        // Structured address fields (PSGC-based)
        [MaxLength(50)]
        public string IslandGroup { get; set; }

        [MaxLength(100)]
        public string Region { get; set; }

        [MaxLength(100)]
        public string? Province { get; set; }

        [MaxLength(150)]
        public string CityMunicipality { get; set; }

        [MaxLength(150)]
        public string Barangay { get; set; }

        [MaxLength(255)]
        public string? StreetAddress { get; set; }

        [MaxLength(10)]
        public string? ZipCode { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
