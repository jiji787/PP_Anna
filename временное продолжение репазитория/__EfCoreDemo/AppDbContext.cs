using Microsoft.EntityFrameworkCore;

namespace EfCoreDemo;

public class AppDbContext : DbContext
{
    // Конструктор без параметров (используется в основном приложении)
    public AppDbContext() { }

    // Конструктор с параметрами (используется в тестах для InMemory)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderStatus> OrderStatuses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Этот метод вызывается только если не переданы параметры в конструктор
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Золотце;Username=postgres;Password=postgres890");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<OrderStatus>().HasData(
            new OrderStatus { Id = 1, Name = "новый" },
            new OrderStatus { Id = 2, Name = "оплачен" },
            new OrderStatus { Id = 3, Name = "отправлен" },
            new OrderStatus { Id = 4, Name = "завершён" }
        );
    }
}