using System.Security.Claims;
using DoctorBook.Data;
using DoctorBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DoctorBook.Controllers;
[Authorize(Roles="Patient")]
public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _db;
    public AppointmentsController(ApplicationDbContext db)=>_db=db;
    [HttpGet] public async Task<IActionResult> Create(int doctorId)
    {
        var doctor=await _db.Doctors.FirstOrDefaultAsync(x=>x.Id==doctorId && x.IsApproved);
        if(doctor==null)return NotFound();
        ViewBag.Doctor=doctor;
        return View(new Appointment{DoctorProfileId=doctorId,AppointmentDate=DateTime.Today.AddDays(1)});
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment m)
    {
        var doctor=await _db.Doctors.Include(x=>x.Schedules).FirstOrDefaultAsync(x=>x.Id==m.DoctorProfileId && x.IsApproved);
        if(doctor==null)return NotFound();
        ModelState.Remove(nameof(m.Doctor)); ModelState.Remove(nameof(m.Payment));
        if(m.AppointmentDate.Date<DateTime.Today) ModelState.AddModelError(nameof(m.AppointmentDate),"اختر تاريخًا صحيحًا.");
        var schedule=doctor.Schedules.FirstOrDefault(s=>s.IsActive && s.DayOfWeek==(int)m.AppointmentDate.DayOfWeek);
        if(schedule==null || m.AppointmentTime<schedule.StartTime || m.AppointmentTime>=schedule.EndTime ||
           ((m.AppointmentTime-schedule.StartTime).TotalMinutes % schedule.SlotMinutes)!=0)
            ModelState.AddModelError(nameof(m.AppointmentTime),"الوقت غير موجود ضمن مواعيد الطبيب المتاحة.");
        var occupied=await _db.Appointments.AnyAsync(a=>a.DoctorProfileId==m.DoctorProfileId && a.AppointmentDate.Date==m.AppointmentDate.Date && a.AppointmentTime==m.AppointmentTime && a.Status!="Cancelled");
        if(occupied) ModelState.AddModelError(nameof(m.AppointmentTime),"هذا الموعد محجوز بالفعل.");
        if(!ModelState.IsValid){ViewBag.Doctor=doctor;return View(m);}
        m.PatientUserId=User.FindFirstValue(ClaimTypes.NameIdentifier); m.Status="Pending";
        _db.Appointments.Add(m);
        await _db.SaveChangesAsync();
        _db.Payments.Add(new PaymentRecord{AppointmentId=m.Id,Amount=doctor.ConsultationPrice,Method="CashAtClinic",Status="Unpaid"});
        _db.Notifications.Add(new SiteNotification{UserId=m.PatientUserId!,Title="تم استلام طلب الحجز",Message=$"طلبك مع {doctor.FullName} قيد المراجعة."});
        if(!string.IsNullOrEmpty(doctor.UserId)) _db.Notifications.Add(new SiteNotification{UserId=doctor.UserId,Title="طلب حجز جديد",Message=$"لديك طلب حجز جديد من {m.PatientName}."});
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(MyAppointments));
    }
    public async Task<IActionResult> MyAppointments()
    {
        var id=User.FindFirstValue(ClaimTypes.NameIdentifier);
        return View(await _db.Appointments.Include(x=>x.Doctor).Include(x=>x.Payment).Where(x=>x.PatientUserId==id).OrderByDescending(x=>x.AppointmentDate).ToListAsync());
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var uid=User.FindFirstValue(ClaimTypes.NameIdentifier);
        var a=await _db.Appointments.FirstOrDefaultAsync(x=>x.Id==id && x.PatientUserId==uid);
        if(a!=null && a.Status=="Pending"){a.Status="Cancelled";await _db.SaveChangesAsync();}
        return RedirectToAction(nameof(MyAppointments));
    }
}
