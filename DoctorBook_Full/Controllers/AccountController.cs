using DoctorBook.Data;
using DoctorBook.Models;
using DoctorBook.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBook.Controllers;
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    public AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) { _users=users; _signIn=signIn; }

    [HttpGet] public IActionResult Register() => View(new RegisterViewModel());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel m)
    {
        if (!ModelState.IsValid) return View(m);
        var user = new ApplicationUser { UserName=m.Email, Email=m.Email, FullName=m.FullName, PhoneNumber=m.PhoneNumber };
        var result = await _users.CreateAsync(user, m.Password);
        if (!result.Succeeded) { foreach(var e in result.Errors) ModelState.AddModelError("", e.Description); return View(m); }
        await _users.AddToRoleAsync(user, "Patient");
        await _signIn.SignInAsync(user, isPersistent:false);
        TempData["Message"] = $"تم إنشاء حسابك بنجاح. رقم المستخدم الخاص بك: {user.Id} — احتفظ به.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet] public IActionResult Login(string? returnUrl=null) { ViewBag.ReturnUrl=returnUrl; return View(new LoginViewModel()); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel m, string? returnUrl=null)
    {
        if (!ModelState.IsValid) return View(m);
        var result = await _signIn.PasswordSignInAsync(m.Email, m.Password, m.RememberMe, lockoutOnFailure:true);
        if (!result.Succeeded) { ModelState.AddModelError("", "البريد الإلكتروني أو كلمة المرور غير صحيحة."); return View(m); }
        var user = await _users.FindByEmailAsync(m.Email);
        if (user != null && await _users.IsInRoleAsync(user,"Admin")) return RedirectToAction("Index","Admin");
        if (user != null && await _users.IsInRoleAsync(user,"Doctor")) return RedirectToAction("Index","DoctorDashboard");
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action("Index","Home")!);
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Logout() { await _signIn.SignOutAsync(); return RedirectToAction("Index","Home"); }
    public IActionResult AccessDenied() => View();
}
