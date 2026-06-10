using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EfCoreDemo;

public class Order
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }
}