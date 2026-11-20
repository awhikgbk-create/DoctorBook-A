using System.Security.Claims;
using DoctorBook.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DoctorBook.Controllers;
[Authorize]
public class NotificationsController:Controller
{
    private readonly ApplicationDbContext _db;
    public NotificationsController(ApplicationDbContext db)=>_db=db;
    public async Task<IActionResult> Index()
    {
        var uid=User.FindFirstValue(ClaimTypes.NameIdentifier);
        return View(await _db.Notifications.Where(x=>x.UserId==uid).OrderByDescending(x=>x.CreatedAt).ToListAsync());
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Read(int id)
    {
        var uid=User.FindFirstValue(ClaimTypes.NameIdentifier);
        var n=await _db.Notifications.FirstOrDefaultAsync(x=>x.Id==id && x.UserId==uid);
        if(n!=null){n.IsRead=true;await _db.SaveChangesAsync();}
        return RedirectToAction(nameof(Index));
    }
}
