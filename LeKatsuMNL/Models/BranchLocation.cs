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

        [Required]
        [MaxLength(255)]
        public string BranchLocationAddress { get; set; }

        // Navigation Properties
        public ICollection<BranchManager> BranchManagers { get; set; } = new List<BranchManager>();
    }
}
