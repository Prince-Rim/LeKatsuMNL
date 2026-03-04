using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class OrderComment
    {
        [Key]
        public int CommentId { get; set; }

        public int OrderId { get; set; }
        public OrderInfo OrderInfo { get; set; }

        public int BranchManagerId { get; set; }
        public BranchManager BranchManager { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Comment { get; set; }
    }
}
