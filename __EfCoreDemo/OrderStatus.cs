using System.ComponentModel.DataAnnotations;

namespace EfCoreDemo;

public class OrderStatus
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}