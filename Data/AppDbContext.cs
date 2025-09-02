using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using webapplication.Models.Entities;

namespace webapplication.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Add your DbSets for other entities
        // public DbSet<Product> Products { get; set; }
        // public DbSet<Order> Orders { get; set; }
    }
}
