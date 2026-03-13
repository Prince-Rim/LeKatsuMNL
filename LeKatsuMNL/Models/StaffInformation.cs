using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Models
{
    public class StaffInformation
    {
        [Key]
        public int StaffId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string MiddleName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(20)]
        public string ContactNum { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        // Navigation Properties
        public ICollection<StaffTimeSlot> StaffTimeSlots { get; set; } = new List<StaffTimeSlot>();
        public ICollection<RestaurantTransaction> RestaurantTransactions { get; set; } = new List<RestaurantTransaction>();
        public ICollection<RestaurantInventory> RestaurantInventories { get; set; } = new List<RestaurantInventory>();
    }
}
