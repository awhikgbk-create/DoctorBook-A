using DoctorBook.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DoctorBook.Controllers;
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    public HomeController(ApplicationDbContext db)=>_db=db;
    public async Task<IActionResult> Index(string? specialty,string? city)
    {
        var q=_db.Doctors.Where(x=>x.IsApproved).AsQueryable();
        if(!string.IsNullOrWhiteSpace(specialty)) q=q.Where(x=>x.Specialty.Contains(specialty));
        if(!string.IsNullOrWhiteSpace(city)) q=q.Where(x=>x.City.Contains(city));
        ViewBag.Specialty=specialty; ViewBag.City=city;
        return View(await q.OrderBy(x=>x.FullName).ToListAsync());
    }
    public IActionResult Error()=>View();
}
