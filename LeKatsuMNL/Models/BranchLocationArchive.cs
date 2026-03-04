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

        [MaxLength(255)]
        public string BranchLocationAddress { get; set; }
    }
}
