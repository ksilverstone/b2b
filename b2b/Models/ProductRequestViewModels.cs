using System;

namespace b2b.Models
{
    public class ProductRequestListItem
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}


