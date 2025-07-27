namespace b2b.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsSeller { get; set; }
        public bool IsBuyer { get; set; }
        public ICollection<User>? Users { get; set; }
    }
}