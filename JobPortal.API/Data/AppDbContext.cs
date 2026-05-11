using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DanhMuc> DanhMucs { get; set; } = null!;
    public DbSet<QuanTriVien> QuanTriViens { get; set; } = null!;
    public DbSet<NhaTuyenDung> NhaTuyenDungs { get; set; } = null!;
    public DbSet<NguoiTimViec> NguoiTimViecs { get; set; } = null!;
    public DbSet<GoiBaiDang> GoiBaiDangs { get; set; } = null!;
    public DbSet<CongViec> CongViecs { get; set; } = null!;
    public DbSet<HoSoTuyenDung> HoSoTuyenDungs { get; set; } = null!;
    public DbSet<HinhAnh> HinhAnhs { get; set; } = null!;
    public DbSet<UngTuyen> UngTuyens { get; set; } = null!;
    public DbSet<QuaTrinhLamViec> QuaTrinhLamViecs { get; set; } = null!;
    public DbSet<DanhGia> DanhGias { get; set; } = null!;
    public DbSet<LichSuThanhToan> LichSuThanhToans { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GoiBaiDang>()
            .HasOne(g => g.QuanTriVien)
            .WithMany(q => q.GoiBaiDangs)
            .HasForeignKey(g => g.IdQuanTri)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CongViec>()
            .HasOne(c => c.NhaTuyenDung)
            .WithMany(n => n.CongViecs)
            .HasForeignKey(c => c.IdTuyenDung)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CongViec>()
            .HasOne(c => c.DanhMuc)
            .WithMany(d => d.CongViecs)
            .HasForeignKey(c => c.IdDanhMuc)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HoSoTuyenDung>()
            .HasOne(h => h.NguoiTimViec)
            .WithMany(n => n.HoSoTuyenDungs)
            .HasForeignKey(h => h.IdTimViec)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HinhAnh>()
            .HasOne(h => h.CongViec)
            .WithMany(c => c.HinhAnhs)
            .HasForeignKey(h => h.IdCongViec)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UngTuyen>()
            .HasOne(u => u.NguoiTimViec)
            .WithMany(n => n.UngTuyens)
            .HasForeignKey(u => u.IdTimViec)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UngTuyen>()
            .HasOne(u => u.CongViec)
            .WithMany(c => c.UngTuyens)
            .HasForeignKey(u => u.IdCongViec)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UngTuyen>()
            .HasOne(u => u.HoSoTuyenDung)
            .WithMany(h => h.UngTuyens)
            .HasForeignKey(u => u.IdCv)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuaTrinhLamViec>()
            .HasOne(q => q.UngTuyen)
            .WithMany(u => u.QuaTrinhLamViecs)
            .HasForeignKey(q => q.IdUngTuyen)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DanhGia>()
            .HasOne(d => d.CongViec)
            .WithMany(c => c.DanhGias)
            .HasForeignKey(d => d.IdCongViec)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DanhGia>()
            .HasOne(d => d.NhaTuyenDung)
            .WithMany(n => n.DanhGias)
            .HasForeignKey(d => d.IdTuyenDung)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DanhGia>()
            .HasOne(d => d.NguoiTimViec)
            .WithMany(n => n.DanhGias)
            .HasForeignKey(d => d.IdTimViec)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LichSuThanhToan>()
            .HasOne(l => l.NhaTuyenDung)
            .WithMany(n => n.LichSuThanhToans)
            .HasForeignKey(l => l.IdTuyenDung)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LichSuThanhToan>()
            .HasOne(l => l.GoiBaiDang)
            .WithMany(g => g.LichSuThanhToans)
            .HasForeignKey(l => l.IdGoi)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
