using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        public int OrderId { get; set; }
        public OrderInfo OrderInfo { get; set; }

        public DateTime InvoiceDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        public DateTime? PaymentDate { get; set; }

        [MaxLength(100)]
        public string? VerifiedBy { get; set; }
    }
}
