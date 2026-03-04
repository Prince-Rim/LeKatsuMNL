using System;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class STimeArchive
    {
        [Key]
        public int StaId { get; set; }

        public int TimeId { get; set; }

        public int StaffId { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan TimeIn { get; set; }

        public TimeSpan? TimeOut { get; set; }
    }
}
