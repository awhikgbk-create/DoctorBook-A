using DoctorBook.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DoctorBook.Controllers;
public class DoctorsController : Controller
{
    private readonly ApplicationDbContext _db;
    public DoctorsController(ApplicationDbContext db)=>_db=db;
    public async Task<IActionResult> Index(string? specialty,string? city)
    {
        var q=_db.Doctors.Where(x=>x.IsApproved).AsQueryable();
        if(!string.IsNullOrWhiteSpace(specialty)) q=q.Where(x=>x.Specialty.Contains(specialty));
        if(!string.IsNullOrWhiteSpace(city)) q=q.Where(x=>x.City.Contains(city));
        return View(await q.ToListAsync());
    }
    public async Task<IActionResult> Details(int id)
    {
        var d=await _db.Doctors.Include(x=>x.Schedules).FirstOrDefaultAsync(x=>x.Id==id && x.IsApproved);
        return d==null?NotFound():View(d);
    }
}
