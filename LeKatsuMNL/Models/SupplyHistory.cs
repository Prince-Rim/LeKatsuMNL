using System;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class SupplyHistory
    {
        [Key]
        public int SHistoryId { get; set; }

        public int SupplyId { get; set; }
        public SupplyOrder SupplyOrder { get; set; }
    }
}
