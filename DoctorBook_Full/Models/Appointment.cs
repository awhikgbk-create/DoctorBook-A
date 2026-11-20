using System.ComponentModel.DataAnnotations;
namespace DoctorBook.Models;
public class Appointment
{
    public int Id { get; set; }
    [Required] public int DoctorProfileId { get; set; }
    public DoctorProfile? Doctor { get; set; }
    [Required, StringLength(120)] public string PatientName { get; set; } = "";
    [Required, Phone, StringLength(30)] public string PatientPhone { get; set; } = "";
    [Required] public DateTime AppointmentDate { get; set; }
    [Required] public TimeSpan AppointmentTime { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
    [StringLength(30)] public string Status { get; set; } = "Pending";
    public string? PatientUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public PaymentRecord? Payment { get; set; }
}
