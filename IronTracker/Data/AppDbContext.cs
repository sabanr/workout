using Microsoft.EntityFrameworkCore;
using IronTracker.Models;

namespace IronTracker.Data;

/// <summary>
/// Entity Framework Core DbContext for IronTracker application.
/// Configured for SQLite with cross-platform support.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Routine> Routines => Set<Routine>();
    public DbSet<RoutineDay> RoutineDays => Set<RoutineDay>();
    public DbSet<ExerciseTemplate> ExerciseTemplates => Set<ExerciseTemplate>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<SetLog> SetLogs => Set<SetLog>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Routine configuration
        modelBuilder.Entity<Routine>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Description).HasMaxLength(500);
            entity.HasMany(r => r.Days)
                  .WithOne(d => d.Routine)
                  .HasForeignKey(d => d.RoutineId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // RoutineDay configuration
        modelBuilder.Entity<RoutineDay>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            entity.HasMany(d => d.Exercises)
                  .WithOne(e => e.RoutineDay)
                  .HasForeignKey(e => e.RoutineDayId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(d => d.Sessions)
                  .WithOne(s => s.RoutineDay)
                  .HasForeignKey(s => s.RoutineDayId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ExerciseTemplate configuration
        modelBuilder.Entity<ExerciseTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TargetConfig).IsRequired().HasMaxLength(50);
        });

        // WorkoutSession configuration
        modelBuilder.Entity<WorkoutSession>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Notes).HasMaxLength(1000);
            entity.HasMany(s => s.SetLogs)
                  .WithOne(l => l.WorkoutSession)
                  .HasForeignKey(l => l.WorkoutSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // SetLog configuration
        modelBuilder.Entity<SetLog>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.ExerciseName).IsRequired().HasMaxLength(100);
            entity.Property(l => l.WeightUsed).HasPrecision(10, 2);
            
            // Index for efficient querying by date
            entity.HasIndex(l => l.CompletedAt);
            
            // Index for querying by session and exercise
            entity.HasIndex(l => new { l.WorkoutSessionId, l.ExerciseName });
        });
    }

    /// <summary>
    /// Gets the cross-platform database path.
    /// Uses FileSystem.AppDataDirectory for consistent behavior across all platforms.
    /// </summary>
    public static string GetDatabasePath()
    {
        return Path.Combine(FileSystem.AppDataDirectory, "irontracker.db");
    }
}
