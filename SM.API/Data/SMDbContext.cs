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

            // Başlangıç verileri (Seed Data) - DateTime.Now yerine sabit tarih
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = 1, Name = "Ana Depo", Location = "İstanbul", CreatedDate = new DateTime(2024, 1, 1) },
                new Warehouse { Id = 2, Name = "İkinci Depo", Location = "Ankara", CreatedDate = new DateTime(2024, 1, 1) }
            );
        }
    }
}