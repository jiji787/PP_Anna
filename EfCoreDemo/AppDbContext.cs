using Microsoft.EntityFrameworkCore;

namespace EfCoreDemo;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Используй ТУ ЖЕ строку подключения, что и в основном приложении
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Золотце;Username=postgres;Password=postgres890");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Дополнительные настройки, если нужно
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId);
    }
}