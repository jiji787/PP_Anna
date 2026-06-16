using System.ComponentModel.DataAnnotations;

namespace EfCoreDemo;

public class User
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public int? Age { get; set; }
    public string City { get; set; }
    public string Email { get; set; }
}