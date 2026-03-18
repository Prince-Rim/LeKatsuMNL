using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class OrderInfo
    {
        [Key]
        public int OrderId { get; set; }

        public int BranchManagerId { get; set; }
        public BranchManager BranchManager { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        // Navigation Properties
        public ICollection<OrderList> OrderLists { get; set; } = new List<OrderList>();
        public ICollection<OrderComment> OrderComments { get; set; } = new List<OrderComment>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        public bool IsArchived { get; set; } = false;
    }
}
