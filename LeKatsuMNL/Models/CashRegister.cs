using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeKatsuMNL.Models
{
    public class CashRegister
    {
        [Key]
        public int CrId { get; set; }

        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal StartingCash { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentCash { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AddedCash { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CashSales { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CashExpenses { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? NonCashExpenses { get; set; }

        // Navigation Properties
        public ICollection<CashAdded> CashAddeds { get; set; } = new List<CashAdded>();
        public ICollection<CashExpense> CashExpensesList { get; set; } = new List<CashExpense>();
    }
}
