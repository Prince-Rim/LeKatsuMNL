using System;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class BranchManagerArchive
    {
        [Key]
        public int BmaId { get; set; }

        public int ManagerId { get; set; }

        [MaxLength(255)]
        public string Password { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string MiddleName { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(20)]
        public string ContactNum { get; set; }

        [MaxLength(20)]
        public string Status { get; set; }
    }
}
