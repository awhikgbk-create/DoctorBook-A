using DoctorBook.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace DoctorBook.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<DoctorProfile> Doctors => Set<DoctorProfile>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<PaymentRecord> Payments => Set<PaymentRecord>();
    public DbSet<SiteNotification> Notifications => Set<SiteNotification>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<DoctorProfile>().HasMany(x => x.Appointments).WithOne(x => x.Doctor).HasForeignKey(x => x.DoctorProfileId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<DoctorProfile>().HasMany(x => x.Schedules).WithOne(x => x.Doctor).HasForeignKey(x => x.DoctorProfileId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Appointment>().HasOne(x => x.Payment).WithOne(x => x.Appointment).HasForeignKey<PaymentRecord>(x => x.AppointmentId);
        b.Entity<Appointment>().HasIndex(x => new { x.DoctorProfileId, x.AppointmentDate, x.AppointmentTime });
    }
}
