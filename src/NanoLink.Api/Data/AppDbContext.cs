using Microsoft.EntityFrameworkCore;
using NanoLink.Api.Data.Entities;

namespace NanoLink.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Link> Links => Set<Link>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
