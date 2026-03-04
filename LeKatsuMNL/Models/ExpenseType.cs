using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class ExpenseType
    {
        [Key]
        public int ExpenseId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; }

        // Navigation Properties
        public ICollection<CashExpense> CashExpenses { get; set; } = new List<CashExpense>();
    }
}
