using System;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class StaffArchive
    {
        [Key]
        public int AsId { get; set; }

        public int StaffId { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string MiddleName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(20)]
        public string ContactNum { get; set; }

        [MaxLength(20)]
        public string Status { get; set; }
    }
}
