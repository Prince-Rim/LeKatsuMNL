using System;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class SubBranchOrderArchive
    {
        [Key]
        public int SboId { get; set; }

        public int OrderId { get; set; }

        public int BManagerId { get; set; }

        public int ListId { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }
    }
}
