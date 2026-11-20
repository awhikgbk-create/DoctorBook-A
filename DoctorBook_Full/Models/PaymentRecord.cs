using System.ComponentModel.DataAnnotations;
namespace DoctorBook.Models;
public class PaymentRecord
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    [Range(0, 1000000)] public decimal Amount { get; set; }
    [Required, StringLength(30)] public string Method { get; set; } = "CashAtClinic";
    [Required, StringLength(30)] public string Status { get; set; } = "Unpaid";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
