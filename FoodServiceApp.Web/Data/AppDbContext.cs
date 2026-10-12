using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Models.Entities;

namespace FoodServiceApp.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
        public DbSet<GianHang> GianHangs => Set<GianHang>();
        public DbSet<QuanTri> QuanTris => Set<QuanTri>();
        public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
        public DbSet<TaiXe> TaiXes => Set<TaiXe>();
        public DbSet<DanhMuc> DanhMucs => Set<DanhMuc>();
        public DbSet<MonAn> MonAns => Set<MonAn>();
        public DbSet<DonHang> DonHangs => Set<DonHang>();
        public DbSet<ChiTietDonHang> ChiTietDonHangs => Set<ChiTietDonHang>();
        public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
        public DbSet<KhuyenMai> KhuyenMais => Set<KhuyenMai>();
        public DbSet<DanhGia> DanhGias => Set<DanhGia>();
        public DbSet<GiaoHang> GiaoHangs => Set<GiaoHang>();
        public DbSet<DanhGiaMon> DanhGiaMons => Set<DanhGiaMon>();
        public DbSet<DiaChiKhachHang> DiaChiKhachHangs => Set<DiaChiKhachHang>();
        public DbSet<PhanHoi> PhanHois => Set<PhanHoi>();
        public DbSet<TaiXeTuChoiDon> TaiXeTuChoiDons => Set<TaiXeTuChoiDon>();
        public DbSet<CauHinhGianHang> CauHinhGianHangs => Set<CauHinhGianHang>();
        public DbSet<ChienDichQuangCao> ChienDichQuangCaos => Set<ChienDichQuangCao>();
        public DbSet<SuDungKhuyenMai> SuDungKhuyenMais => Set<SuDungKhuyenMai>();
        public DbSet<NhatKyTrangThaiDon> NhatKyTrangThaiDons => Set<NhatKyTrangThaiDon>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GianHang>()
                .HasOne(g => g.TaiKhoan)
                .WithMany()
                .HasForeignKey(g => g.MaTK);

            modelBuilder.Entity<DanhMuc>()
                .HasOne(d => d.GianHang)
                .WithMany(g => g.DanhMucs)
                .HasForeignKey(d => d.MaGianHang);

            modelBuilder.Entity<CauHinhGianHang>()
                .HasOne(c => c.GianHang)
                .WithOne(g => g.CauHinh)
                .HasForeignKey<CauHinhGianHang>(c => c.MaGianHang);

            modelBuilder.Entity<ChienDichQuangCao>()
                .HasOne(c => c.GianHang)
                .WithMany(g => g.ChienDichQuangCaos)
                .HasForeignKey(c => c.MaGianHang);

            modelBuilder.Entity<ChienDichQuangCao>().Property(c => c.NganSach).HasPrecision(12, 0);
            modelBuilder.Entity<ChienDichQuangCao>().HasIndex(c => new { c.MaGianHang, c.TrangThai, c.NgayBatDau, c.NgayKetThuc });
            modelBuilder.Entity<DanhMuc>().HasIndex(c => new { c.MaGianHang, c.ThuTuHienThi });

            modelBuilder.Entity<MonAn>()
                .HasOne(m => m.DanhMuc)
                .WithMany(d => d.MonAns)
                .HasForeignKey(m => m.MaDanhMuc);

            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.GianHang)
                .WithMany(g => g.DonHangs)
                .HasForeignKey(d => d.MaGianHang);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(c => c.MonAn)
                .WithMany()
                .HasForeignKey(c => c.MaMonAn)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<NhatKyTrangThaiDon>()
                .HasOne(x => x.DonHang).WithMany().HasForeignKey(x => x.MaDonHang).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<NhatKyTrangThaiDon>().HasIndex(x => new { x.MaDonHang, x.ThoiGian });

            modelBuilder.Entity<DonHang>()
                .HasMany(d => d.ChiTiets)
                .WithOne()
                .HasForeignKey(c => c.MaDonHang)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ThanhToan>()
                .HasOne<DonHang>()
                .WithOne(d => d.ThanhToan)
                .HasForeignKey<ThanhToan>(t => t.MaDonHang);

            modelBuilder.Entity<KhuyenMai>()
                .HasOne<GianHang>()
                .WithMany(g => g.KhuyenMais)
                .HasForeignKey(k => k.MaGianHang);

            modelBuilder.Entity<SuDungKhuyenMai>()
                .HasOne(x => x.KhuyenMai).WithMany().HasForeignKey(x => x.MaKhuyenMai).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<SuDungKhuyenMai>().HasIndex(x => new { x.MaKhuyenMai, x.MaKH });
            modelBuilder.Entity<SuDungKhuyenMai>().HasIndex(x => x.MaDonHang).IsUnique();
            modelBuilder.Entity<KhuyenMai>().Property(x => x.GiaTriDonToiThieu).HasPrecision(12, 0);
            modelBuilder.Entity<KhuyenMai>().Property(x => x.GiamPhiVanChuyenToiDa).HasPrecision(12, 0);

            modelBuilder.Entity<GiaoHang>()
                .HasOne(g => g.DonHang)
                .WithOne(d => d.GiaoHang)
                .HasForeignKey<GiaoHang>(g => g.MaDonHang);

            modelBuilder.Entity<GiaoHang>()
                .HasOne(g => g.TaiXe)
                .WithMany(t => t.GiaoHangs)
                .HasForeignKey(g => g.MaTaiXe);

            modelBuilder.Entity<DanhGiaMon>()
                .HasOne(d => d.MonAn)
                .WithMany()
                .HasForeignKey(d => d.MaMonAn);

            modelBuilder.Entity<PhanHoi>()
                .HasOne(p => p.KhachHang)
                .WithMany()
                .HasForeignKey(p => p.MaKH);

            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.KhuyenMaiApDung)
                .WithMany()
                .HasForeignKey(d => d.MaKhuyenMaiApDung)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
