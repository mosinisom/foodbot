using Microsoft.EntityFrameworkCore;

public class DataContext : DbContext
{
    private readonly IConfiguration _configuration;

    public DataContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("ConnectionString"));
    }

    public DbSet<User> User { get; set; }
    public DbSet<Component> Component { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    modelBuilder.Entity<User>()
        .ToTable("users")
        .HasKey(u => u.user_id);

    modelBuilder.Entity<Component>()
        .ToTable("components")
        .HasKey(c => c.component_id);
    }

}