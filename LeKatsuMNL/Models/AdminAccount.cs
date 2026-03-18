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

        [MaxLength(255)]
        public string? Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public string? Privileges { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Admin";

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        [MaxLength(20)]
        public string? ContactNum { get; set; }

        public bool IsSuperAdmin { get; set; }
    }
}
