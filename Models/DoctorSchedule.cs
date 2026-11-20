using System.ComponentModel.DataAnnotations;
namespace DoctorBook.Models;
public class DoctorSchedule
{
    public int Id { get; set; }
    public int DoctorProfileId { get; set; }
    public DoctorProfile? Doctor { get; set; }
    [Range(0, 6)] public int DayOfWeek { get; set; }
    [Required] public TimeSpan StartTime { get; set; }
    [Required] public TimeSpan EndTime { get; set; }
    [Range(5, 240)] public int SlotMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}
