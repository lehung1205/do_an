using JobPortal.API.Models;
using JobPortal.API.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Admin> Admins { get; set; } = null!;
    public DbSet<Employer> Employers { get; set; } = null!;
    public DbSet<JobSeeker> JobSeekers { get; set; } = null!;
    public DbSet<PostingPackage> PostingPackages { get; set; } = null!;
    public DbSet<Job> Jobs { get; set; } = null!;
    public DbSet<Resume> Resumes { get; set; } = null!;
    public DbSet<Image> Images { get; set; } = null!;
    public DbSet<Application> Applications { get; set; } = null!;
    public DbSet<WorkExperience> WorkExperiences { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<PaymentHistory> PaymentHistories { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostingPackage>()
            .HasOne(g => g.Admin)
            .WithMany(q => q.PostingPackages)
            .HasForeignKey(g => g.AdminId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Job>()
            .HasOne(c => c.Employer)
            .WithMany(n => n.Jobs)
            .HasForeignKey(c => c.EmployerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Job>()
            .HasOne(c => c.Category)
            .WithMany(d => d.Jobs)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Resume>()
            .HasOne(h => h.JobSeeker)
            .WithMany(n => n.Resumes)
            .HasForeignKey(h => h.JobSeekerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Image>()
            .HasOne(h => h.Job)
            .WithMany(c => c.Images)
            .HasForeignKey(h => h.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Application>()
            .HasOne(u => u.JobSeeker)
            .WithMany(n => n.Applications)
            .HasForeignKey(u => u.JobSeekerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Application>()
            .HasOne(u => u.Job)
            .WithMany(c => c.Applications)
            .HasForeignKey(u => u.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Application>()
            .HasOne(u => u.Resume)
            .WithMany(h => h.Applications)
            .HasForeignKey(u => u.ResumeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkExperience>()
            .HasOne(q => q.Application)
            .WithMany(u => u.WorkExperiences)
            .HasForeignKey(q => q.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(d => d.Job)
            .WithMany(c => c.Reviews)
            .HasForeignKey(d => d.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(d => d.Employer)
            .WithMany(n => n.Reviews)
            .HasForeignKey(d => d.EmployerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(d => d.JobSeeker)
            .WithMany(n => n.Reviews)
            .HasForeignKey(d => d.JobSeekerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PaymentHistory>()
            .HasOne(l => l.Employer)
            .WithMany(n => n.PaymentHistories)
            .HasForeignKey(l => l.EmployerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PaymentHistory>()
            .HasOne(l => l.PostingPackage)
            .WithMany(g => g.PaymentHistories)
            .HasForeignKey(l => l.PostingPackageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Admin>()
            .HasOne(a => a.User)
            .WithOne(u => u.AdminProfile)
            .HasForeignKey<Admin>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Employer>()
            .HasOne(e => e.User)
            .WithOne(u => u.EmployerProfile)
            .HasForeignKey<Employer>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JobSeeker>()
            .HasOne(js => js.User)
            .WithOne(u => u.JobSeekerProfile)
            .HasForeignKey<JobSeeker>(js => js.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
