using Microsoft.EntityFrameworkCore;

namespace b2b.Models
{
    public class B2BContext : DbContext
    {
        public B2BContext(DbContextOptions<B2BContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add default roles
            modelBuilder.Entity<Role>().HasData(new Role
            {
                Id = 1,
                RoleName = "Admin"
            }, new Role
            {
                Id = 2,
                RoleName = "Seller"
            }, new Role
            {
                Id = 3,
                RoleName = "Buyer"
            });
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
        }
    }
}