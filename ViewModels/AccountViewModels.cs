using System.ComponentModel.DataAnnotations;
namespace DoctorBook.ViewModels;
public class RegisterViewModel
{
    [Required, StringLength(120)] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, Phone] public string PhoneNumber { get; set; } = "";
    [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
}
public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}

public class PromoteDoctorViewModel
{
    [Required] public string PublicId { get; set; } = "";
    [Required, StringLength(120)] public string FullName { get; set; } = "";
    [Required, StringLength(100)] public string Specialty { get; set; } = "";
    [Required, StringLength(100)] public string City { get; set; } = "";
    [StringLength(1500)] public string? Bio { get; set; }
    [Range(0, 100000)] public decimal ConsultationPrice { get; set; }
    [StringLength(150)] public string? ClinicAddress { get; set; }
    [Phone, StringLength(30)] public string? PublicPhone { get; set; }
}
