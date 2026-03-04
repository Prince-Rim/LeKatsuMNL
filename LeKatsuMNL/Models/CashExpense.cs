using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class CashExpense
    {
        [Key]
        public int CshExpId { get; set; }

        public int CrId { get; set; }
        public CashRegister CashRegister { get; set; }

        public DateTime DateTime { get; set; }

        public int ExpenseId { get; set; }
        public ExpenseType ExpenseType { get; set; }

        [Required]
        [MaxLength(150)]
        public string ExpenseName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpenseAmount { get; set; }

        
    }
}
