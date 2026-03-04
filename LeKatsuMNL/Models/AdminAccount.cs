using System;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class AdminAccount
    {
        [Key]
        public int ManagerId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string MiddleName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Privileges { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        public bool IsSuperAdmin { get; set; }
    }
}
