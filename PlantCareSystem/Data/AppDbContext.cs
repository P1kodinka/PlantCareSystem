using Microsoft.EntityFrameworkCore;
using PlantCareSystem.Models;

namespace PlantCareSystem.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Plant> Plants { get; set; }
        public DbSet<CareOperation> CareOperations { get; set; }
        public DbSet<CareSchedule> CareSchedules { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связей и ограничений
            modelBuilder.Entity<CareOperation>()
                .HasOne(o => o.Plant)
                .WithMany(p => p.CareOperations)
                .HasForeignKey(o => o.PlantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CareSchedule>()
                .HasOne(s => s.Plant)
                .WithMany(p => p.CareSchedules)
                .HasForeignKey(s => s.PlantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Преобразование enum в строку для удобочитаемости
            modelBuilder.Entity<CareOperation>()
                .Property(o => o.OperationType)
                .HasConversion<string>();

            modelBuilder.Entity<CareSchedule>()
                .Property(s => s.OperationType)
                .HasConversion<string>();

            // Индексы для ускорения поиска
            modelBuilder.Entity<Plant>()
                .HasIndex(p => p.Name);

            modelBuilder.Entity<CareOperation>()
                .HasIndex(o => new { o.PlantId, o.OperationDate });

            modelBuilder.Entity<CareSchedule>()
                .HasIndex(s => new { s.PlantId, s.OperationType });
        }
    }
}