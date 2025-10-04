using Microsoft.EntityFrameworkCore;
using MiniUrl.Database.Configurations;
using MiniUrl.Entities;

namespace MiniUrl.Database;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<TinyUrl> TinyUrls { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TinyUrlConfiguration());
    }
}