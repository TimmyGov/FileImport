using FileImportService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileImportService.Infrastructure.Persistence;

/// <summary>
/// Database context for file import staging
/// </summary>
public class StagingDbContext : DbContext
{
    public StagingDbContext(DbContextOptions<StagingDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Staging records table
    /// </summary>
    public DbSet<StagingRecord> FileImportStaging { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StagingRecord>(entity =>
        {
            entity.ToTable("FileImportStaging");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.BatchId)
                .IsRequired();

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.FileType)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.RowNumber)
                .IsRequired();

            entity.Property(e => e.RawData)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.ProcessedStatus)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.ValidationErrors)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.ImportedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            entity.Property(e => e.ProcessedAt)
                .HasColumnType("datetime2");

            // Indexes
            entity.HasIndex(e => e.BatchId)
                .HasDatabaseName("IX_BatchId");

            entity.HasIndex(e => e.ProcessedStatus)
                .HasDatabaseName("IX_ProcessedStatus");
        });
    }
}
