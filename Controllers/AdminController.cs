using DoctorBook.Data;
using DoctorBook.Models;
using DoctorBook.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DoctorBook.Controllers;
[Authorize(Roles="Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> users){_db=db;_users=users;}
    public async Task<IActionResult> Index()
    {
        ViewBag.Doctors=await _db.Doctors.CountAsync();
        ViewBag.PendingDoctors=await _db.Doctors.CountAsync(x=>!x.IsApproved);
        ViewBag.Appointments=await _db.Appointments.CountAsync();
        ViewBag.PendingAppointments=await _db.Appointments.CountAsync(x=>x.Status=="Pending");
        return View(await _db.Appointments.Include(x=>x.Doctor).OrderByDescending(x=>x.CreatedAt).Take(15).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> PromotePatient(string? publicId)
    {
        var vm = new PromoteDoctorViewModel { PublicId = publicId ?? "" };
        if (!string.IsNullOrWhiteSpace(publicId))
        {
            var patient = await _users.Users.FirstOrDefaultAsync(x => x.Id == publicId.Trim());
            if (patient == null) ViewBag.LookupMessage = "لم يتم العثور على مستخدم بهذا الرقم.";
            else if (!await _users.IsInRoleAsync(patient, "Patient")) ViewBag.LookupMessage = "هذا الحساب ليس حساب مريض قابلًا للتحويل.";
            else { vm.FullName = patient.FullName; ViewBag.LookupMessage = $"تم العثور على الحساب: {patient.FullName} ({patient.Email}). أكمل بيانات الملف المهني."; }
        }
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PromotePatient(PromoteDoctorViewModel m)
    {
        var patient = await _users.Users.FirstOrDefaultAsync(x => x.Id == m.PublicId.Trim());
        if (patient == null) { ModelState.AddModelError(nameof(m.PublicId), "رقم المستخدم غير موجود."); return View(m); }
        if (!await _users.IsInRoleAsync(patient, "Patient")) { ModelState.AddModelError(nameof(m.PublicId), "الحساب ليس مريضًا أو تم تحويله من قبل."); return View(m); }
        if (!ModelState.IsValid) return View(m);
        var remove = await _users.RemoveFromRoleAsync(patient, "Patient");
        if (!remove.Succeeded) { ModelState.AddModelError("", "تعذر تحديث صلاحية الحساب."); return View(m); }
        var add = await _users.AddToRoleAsync(patient, "Doctor");
        if (!add.Succeeded) { await _users.AddToRoleAsync(patient, "Patient"); ModelState.AddModelError("", "تعذر إضافة دور الطبيب."); return View(m); }
        _db.Doctors.Add(new DoctorProfile { FullName=m.FullName, Specialty=m.Specialty, City=m.City, Bio=m.Bio ?? "", ConsultationPrice=m.ConsultationPrice, ClinicAddress=m.ClinicAddress, PublicPhone=m.PublicPhone, UserId=patient.Id, IsApproved=true });
        _db.Notifications.Add(new SiteNotification { UserId=patient.Id, Title="تم تفعيل حساب الطبيب", Message="تم تحويل حسابك إلى حساب طبيب. يمكنك الآن الدخول إلى لوحة الطبيب بنفس بيانات الدخول." });
        await _db.SaveChangesAsync();
        TempData["Message"] = "تم تحويل الحساب إلى طبيب وإنشاء ملفه بنجاح.";
        return RedirectToAction(nameof(Doctors));
    }

    public async Task<IActionResult> Doctors()=>View(await _db.Doctors.OrderBy(x=>x.IsApproved).ThenBy(x=>x.FullName).ToListAsync());
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> ApproveDoctor(int id)
    {
        var d=await _db.Doctors.FindAsync(id); if(d!=null){d.IsApproved=true; if(!string.IsNullOrEmpty(d.UserId))_db.Notifications.Add(new SiteNotification{UserId=d.UserId,Title="تم اعتماد حسابك",Message="تمت الموافقة على ملفك وأصبح ظاهرًا في دليل الأطباء."});await _db.SaveChangesAsync();}
        return RedirectToAction(nameof(Doctors));
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> RejectDoctor(int id)
    {
        var d=await _db.Doctors.FindAsync(id); if(d!=null){d.IsApproved=false;await _db.SaveChangesAsync();}
        return RedirectToAction(nameof(Doctors));
    }
    public async Task<IActionResult> Appointments()=>View(await _db.Appointments.Include(x=>x.Doctor).Include(x=>x.Payment).OrderByDescending(x=>x.CreatedAt).ToListAsync());
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> UpdateAppointment(int id,string status)
    {
        var a=await _db.Appointments.FindAsync(id);
        if(a!=null && new[]{"Pending","Confirmed","Cancelled","Completed"}.Contains(status)){a.Status=status;await _db.SaveChangesAsync();}
        return RedirectToAction(nameof(Appointments));
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> MarkPaid(int id)
    {
        var p=await _db.Payments.FirstOrDefaultAsync(x=>x.AppointmentId==id);
        if(p!=null){p.Status="Paid";await _db.SaveChangesAsync();}
        return RedirectToAction(nameof(Appointments));
    }
}
