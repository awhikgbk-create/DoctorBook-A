using System.ComponentModel.DataAnnotations;
namespace DoctorBook.Models;
public class DoctorProfile
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string FullName { get; set; } = "";
    [Required, StringLength(100)] public string Specialty { get; set; } = "";
    [StringLength(100)] public string City { get; set; } = "";
    [StringLength(1500)] public string Bio { get; set; } = "";
    [Range(0, 100000)] public decimal ConsultationPrice { get; set; }
    [StringLength(30)] public string? ClinicAddress { get; set; }
    [Phone, StringLength(30)] public string? PublicPhone { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
