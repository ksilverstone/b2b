using System;
using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class CustomerOrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }

        [StringLength(100)]
        public string? OldStatus { get; set; }

        [Required]
        [StringLength(100)]
        public string NewStatus { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Note { get; set; }

        public int? ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.Now;

        // İlişki alanları
        public CustomerOrder? Order { get; set; }
        public User? ChangedByUser { get; set; }
    }
}


