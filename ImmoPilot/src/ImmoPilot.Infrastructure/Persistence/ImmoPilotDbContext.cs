using ImmoPilot.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImmoPilot.Infrastructure.Persistence;

public class ImmoPilotDbContext(DbContextOptions<ImmoPilotDbContext> options) : DbContext(options)
{
    public DbSet<PropertyEntity> Properties => Set<PropertyEntity>();
    public DbSet<NotificationLogEntity> NotificationLogs => Set<NotificationLogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PropertyEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MlsId).IsUnique();
            entity.Property(e => e.MlsId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(200).IsRequired();
            entity.Property(e => e.City).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Zip).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CapRate).HasColumnType("decimal(8,6)");
            entity.Property(e => e.CocRate).HasColumnType("decimal(8,6)");
            entity.Property(e => e.CashFlow).HasColumnType("decimal(12,2)");
            entity.Property(e => e.CashRequired).HasColumnType("decimal(12,2)");
            entity.Property(e => e.FmrRent).HasColumnType("decimal(10,2)");
            entity.Property(e => e.RehabPercent).HasColumnType("decimal(5,4)");
            entity.Property(e => e.DscrRatio).HasColumnType("decimal(8,4)");
            entity.Property(e => e.DscrStatus).HasConversion<string>();
            entity.Property(e => e.FmrStatus).HasConversion<string>();
            entity.Property(e => e.ListingStatus).HasConversion<string>();
        });

        modelBuilder.Entity<NotificationLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Recipients).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.Property)
                  .WithMany(p => p.NotificationLogs)
                  .HasForeignKey(e => e.PropertyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
