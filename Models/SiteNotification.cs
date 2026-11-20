using System.ComponentModel.DataAnnotations;
namespace DoctorBook.Models;
public class SiteNotification
{
    public int Id { get; set; }
    [Required] public string UserId { get; set; } = "";
    [Required, StringLength(180)] public string Title { get; set; } = "";
    [Required, StringLength(700)] public string Message { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
