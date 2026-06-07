using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<PriceImport> PriceImports { get; set; }
        public DbSet<AccountWarehouseSupervisor> AccountWarehouseSupervisors { get; set; }
        public DbSet<BigPackage> BigPackages { get; set; }
        public DbSet<BigPackageHistory> BigPackageHistories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<OutOfStock> OutOfStocks { get; set; }
        public DbSet<Pricing> Pricings { get; set; }
        public DbSet<PricingSeparate> PricingSeparates { get; set; }
        public DbSet<ReportFixedFee> ReportFixedFees { get; set; }
        public DbSet<ReportOtherFee> ReportOtherFees { get; set; }
        public DbSet<ReportPartnerFee> ReportPartnerFees { get; set; }
        public DbSet<StaffTarget> StaffTargets { get; set; }
        public DbSet<Tracking> Trackings { get; set; }
        public DbSet<Transportation> Transportations { get; set; }
        public DbSet<TransportationHistory> TransportationHistories { get; set; }
        public DbSet<TransportationOutOfStock> TransportationOutOfStocks { get; set; }
        public DbSet<TransportationProduct> TransportationProducts { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherAccount> VoucherAccounts { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<ZaloAPI> ZaloAPIs { get; set; }
        public DbSet<WebConfiguration> WebConfigurations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình các bảng và khóa
            modelBuilder.Entity<TransportationOutOfStock>().HasKey(x => new { x.TransportationId, x.OutOfStockId });

            // Tạo chỉ mục duy nhất
            modelBuilder.Entity<Transportation>().HasIndex(x => x.Barcode).IsUnique();
            modelBuilder.Entity<Account>().HasIndex(x => x.Username).IsUnique();
            modelBuilder.Entity<BigPackage>().HasIndex(x => x.Name).IsUnique();

            // Thêm dữ liệu mẫu chỉ khi cơ sở dữ liệu được tạo lần đầu
            if (!Database.GetAppliedMigrations().Any())
            {
                modelBuilder.Entity<Account>().HasData(
                    new Account { Id = 1, Username = "admin", RoleId = (int)ERoleId.Admin, PasswordHash = "OAejq3Yt3iL9UOjkCsiyzg==", PasswordEncryptValue = "i4HKLM2o6lacp4dC", Created = DateTime.Now, CreatedBy = 1 }
                );

                modelBuilder.Entity<Warehouse>().HasData(
                    new Warehouse { Id = 1, Name = "Kho TQ", Status = (int)EWarehouseStatus.Active, Type = (int)EWarehouseType.Reciever, Created = DateTime.Now, CreatedBy = 1 },
                    new Warehouse { Id = 2, Name = "Kho VN", Status = (int)EWarehouseStatus.Active, Type = (int)EWarehouseType.Destination, Created = DateTime.Now, CreatedBy = 1 },
                    new Warehouse { Id = 3, Name = "Đi thường", Status = (int)EWarehouseStatus.Active, Type = (int)EWarehouseType.Shipping, Created = DateTime.Now, CreatedBy = 1 }
                );

                modelBuilder.Entity<Pricing>().HasData(
                    new Pricing { Id = 1, Created = DateTime.Now, CreatedBy = 1, Type = (int)EPricingType.Weight, RangeMin = 0, RangeMax = 99999999999, PricePerUnit = 10000, FromWarehouseId = 1, ToWarehouseId = 2, ShipId = 3 },
                    new Pricing { Id = 2, Created = DateTime.Now, CreatedBy = 1, Type = (int)EPricingType.Volume, RangeMin = 0, RangeMax = 99999999999, PricePerUnit = 500000, FromWarehouseId = 1, ToWarehouseId = 2, ShipId = 3 }
                );

                modelBuilder.Entity<WebConfiguration>().HasData(
                    new WebConfiguration { Id = 1, WebsiteName = "WebsiteName", WebsiteUrl = "/", Currency = 3650, Created = DateTime.Now, CreatedBy = 1 }
                );
            }
        }
    }
}
