using Microsoft.EntityFrameworkCore;
using NanoLink.Api.Data.Entities;

namespace NanoLink.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Link> Links => Set<Link>();

    public DbSet<ClickEvent> ClickEvents => Set<ClickEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClickEvent>(entity =>
        {
            entity.ToTable("ClickEvents");

            entity.HasKey(click => click.Id);

            entity.Property(click => click.Code)
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(click => click.ClickedAt)
                .IsRequired();

            entity.Property(click => click.IpAddress)
                .HasMaxLength(45);

            entity.Property(click => click.CountryCode)
                .HasMaxLength(2);

            entity.Property(click => click.CountryName)
                .HasMaxLength(100);

            entity.Property(click => click.CityName)
                .HasMaxLength(100);

            entity.Property(click => click.OperatingSystem)
                .HasMaxLength(50);

            entity.Property(click => click.DeviceType)
                .HasMaxLength(30);

            entity.Property(click => click.ReferrerDomain)
                .HasMaxLength(150);

            entity.HasIndex(click => click.Code);

            entity.HasIndex(click => click.ClickedAt);
        });

        modelBuilder.Entity<Link>(entity =>
        {
            entity.ToTable("Links");

            entity.HasKey(link => link.Id);

            entity.Property(link => link.Code)
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(link => link.OriginalUrl)
                .HasMaxLength(2048)
                .IsRequired();

            entity.Property(link => link.CreatedAt)
                .IsRequired();

            // Garante unicidade do código curto no próprio banco
            entity.HasIndex(link => link.Code)
                .IsUnique();
        });
    }
}
