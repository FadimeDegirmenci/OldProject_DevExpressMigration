using Microsoft.EntityFrameworkCore;
using SM.Core.Models;

namespace SM.API.Data
{
    public class SMDbContext : DbContext
    {
        public SMDbContext(DbContextOptions<SMDbContext> options) : base(options)
        {
        }

        // DbSet'ler - veritabanı tablolarımız
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product tablosu yapılandırması
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SKU).HasMaxLength(50);
                entity.HasIndex(e => e.SKU).IsUnique(); // SKU benzersiz olmalı
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            });

            // Warehouse tablosu yapılandırması
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(200);
            });

            // InventoryItem tablosu yapılandırması
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Foreign key ilişkileri
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.InventoryItems)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Warehouse)
                      .WithMany(w => w.InventoryItems)
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Aynı ürün aynı depoda sadece bir kez olabilir
                entity.HasIndex(e => new { e.ProductId, e.WarehouseId }).IsUnique();
            });

            // User tablosu yapılandırması
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Username).IsUnique(); // Kullanıcı adı benzersiz olmalı
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            });

            // Başlangıç verileri (Seed Data) - DateTime.Now yerine sabit tarih
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = 1, Name = "Ana Depo", Location = "İstanbul", CreatedDate = new DateTime(2024, 1, 1) },
                new Warehouse { Id = 2, Name = "İkinci Depo", Location = "Ankara", CreatedDate = new DateTime(2024, 1, 1) }
            );

            // Başlangıç kullanıcıları (test için)
            // Şifreler PasswordHasher formatında önceden hash'lendi:
            //   yonetici / Yonetici123!
            //   depo     / Depo123!
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "yonetici",
                    PasswordHash = "AQAAAAIAAYagAAAAEClKbWHhNAcQjaJijzxVSOd1/9lnh4abMxw7Kzupf5A1GS8Hx59dRq6yVCSGnNEAmA==",
                    Role = "Yonetici"
                },
                new User
                {
                    Id = 2,
                    Username = "depo",
                    PasswordHash = "AQAAAAIAAYagAAAAEJACifbWB7xxH9Hw7eT+VtQre3eA/Y97S3a/BjRPdCSIXAsJ0OvtZlM+TtW3MHPDIA==",
                    Role = "DepoSorumlusu"
                }
            );
        }
    }
}