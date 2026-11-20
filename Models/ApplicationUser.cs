using Microsoft.AspNetCore.Identity;
namespace DoctorBook.Models;
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = "";
}
