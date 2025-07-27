namespace b2b.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = null!;
        public ICollection<UserRole>? UserRoles { get; set; }
    }
}