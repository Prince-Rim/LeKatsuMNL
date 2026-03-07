using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class BranchManager
    {
        [Key]
        public int BManagerId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        public int BranchId { get; set; }
        public BranchLocation BranchLocation { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string MiddleName { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(20)]
        public string ContactNum { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        // Navigation Properties
        public ICollection<OrderInfo> Orders { get; set; } = new List<OrderInfo>();
        public ICollection<OrderComment> OrderComments { get; set; } = new List<OrderComment>();
    }
}
