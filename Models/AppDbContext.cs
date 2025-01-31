using Microsoft.EntityFrameworkCore;

namespace WeatherApp.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Favorite> Favorites { get; set; } = null!;
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
