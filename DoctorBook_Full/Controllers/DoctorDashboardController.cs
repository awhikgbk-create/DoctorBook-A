using System.Security.Claims;
using DoctorBook.Data;
using DoctorBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DoctorBook.Controllers;
[Authorize(Roles="Doctor")]
public class DoctorDashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    public DoctorDashboardController(ApplicationDbContext db)=>_db=db;
    private Task<DoctorProfile?> CurrentDoctor()=>_db.Doctors.Include(x=>x.Schedules).FirstOrDefaultAsync(x=>x.UserId==User.FindFirstValue(ClaimTypes.NameIdentifier));
    public async Task<IActionResult> Index()
    {
        var d=await CurrentDoctor(); if(d==null)return NotFound();
        ViewBag.Doctor=d;
        ViewBag.Pending=await _db.Appointments.CountAsync(x=>x.DoctorProfileId==d.Id && x.Status=="Pending");
        return View(await _db.Appointments.Include(x=>x.Payment).Where(x=>x.DoctorProfileId==d.Id).OrderByDescending(x=>x.AppointmentDate).ToListAsync());
    }
    [HttpGet] public async Task<IActionResult> Profile()
    {
        var d=await CurrentDoctor(); return d==null?NotFound():View(d);
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Profile(DoctorProfile m)
    {
        var d=await CurrentDoctor(); if(d==null)return NotFound();
        ModelState.Remove(nameof(m.UserId)); ModelState.Remove(nameof(m.Appointments)); ModelState.Remove(nameof(m.Schedules));
        if(!ModelState.IsValid)return View(m);
        d.FullName=m.FullName; d.Specialty=m.Specialty; d.City=m.City; d.Bio=m.Bio; d.ConsultationPrice=m.ConsultationPrice; d.ClinicAddress=m.ClinicAddress; d.PublicPhone=m.PublicPhone;
        await _db.SaveChangesAsync(); TempData["Message"]="تم حفظ بيانات الملف الشخصي."; return RedirectToAction(nameof(Profile));
    }
    [HttpGet] public async Task<IActionResult> Schedule()
    {
        var d=await CurrentDoctor(); if(d==null)return NotFound();
        return View(d.Schedules.OrderBy(x=>x.DayOfWeek).ToList());
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> AddSchedule(DoctorSchedule m)
    {
        var d=await CurrentDoctor(); if(d==null)return NotFound();
        if(m.EndTime<=m.StartTime || m.SlotMinutes<5){TempData["Error"]="راجع وقت البداية والنهاية ومدة الكشف.";return RedirectToAction(nameof(Schedule));}
        m.DoctorProfileId=d.Id; _db.DoctorSchedules.Add(m); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Schedule));
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> DeleteSchedule(int id)
    {
        var d=await CurrentDoctor(); if(d==null)return NotFound();
        var s=await _db.DoctorSchedules.FirstOrDefaultAsync(x=>x.Id==id && x.DoctorProfileId==d.Id);
        if(s!=null){_db.Remove(s);await _db.SaveChangesAsync();}
        return RedirectToAction(nameof(Schedule));
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> UpdateStatus(int id,string status)
    {
        var d=await CurrentDoctor(); if(d==null)return NotFound();
        var a=await _db.Appointments.FirstOrDefaultAsync(x=>x.Id==id && x.DoctorProfileId==d.Id);
        if(a!=null && new[]{"Confirmed","Cancelled","Completed"}.Contains(status))
        {
            a.Status=status;
            var patient=a.PatientUserId;
            if(!string.IsNullOrEmpty(patient))_db.Notifications.Add(new SiteNotification{UserId=patient,Title="تحديث حالة الحجز",Message=$"تم تحديث حالة حجزك إلى {status}."});
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
