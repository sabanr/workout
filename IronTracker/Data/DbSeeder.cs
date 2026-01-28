using Microsoft.EntityFrameworkCore;
using IronTracker.Models;

namespace IronTracker.Data;

/// <summary>
/// Seeds the database with initial routine data based on the Excel workout routine.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Seeds the database with sample routine data if no routines exist.
    /// </summary>
    public static async Task SeedAsync(IDbContextFactory<AppDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Only seed if no routines exist
        if (await context.Routines.AnyAsync())
            return;

        var routine = CreateSampleRoutine();
        context.Routines.Add(routine);
        await context.SaveChangesAsync();
    }

    private static Routine CreateSampleRoutine()
    {
        return new Routine
        {
            Name = "Rutina Roberto Saban",
            Description = "Rutina de 3 dias: Push/Pull/Legs",
            CreatedAt = DateTime.UtcNow,
            Days = new List<RoutineDay>
            {
                // Day 1: Chest / Shoulders / Triceps (Push)
                new RoutineDay
                {
                    Name = "Pecho / Hombro / Triceps",
                    SortOrder = 1,
                    Exercises = new List<ExerciseTemplate>
                    {
                        new ExerciseTemplate { Name = "Press Plano", TargetConfig = "15-15-12-10", SortOrder = 1 },
                        new ExerciseTemplate { Name = "Press Inclinado", TargetConfig = "15-15-12-10", SortOrder = 2 },
                        new ExerciseTemplate { Name = "Aperturas", TargetConfig = "10-10-10-10", SortOrder = 3 },
                        new ExerciseTemplate { Name = "Press Arnold", TargetConfig = "12-12-10-10", SortOrder = 4 },
                        new ExerciseTemplate { Name = "Vuelo Lateral", TargetConfig = "12-12-12-12", SortOrder = 5 },
                        new ExerciseTemplate { Name = "Extension Triceps", TargetConfig = "12-12-12-12", SortOrder = 6 },
                        new ExerciseTemplate { Name = "Extension Transnuca", TargetConfig = "12-12-12-12", SortOrder = 7 }
                    }
                },
                // Day 2: Back / Biceps (Pull)
                new RoutineDay
                {
                    Name = "Espalda / Biceps",
                    SortOrder = 2,
                    Exercises = new List<ExerciseTemplate>
                    {
                        new ExerciseTemplate { Name = "Tiron al Pecho", TargetConfig = "15-15-12-10", SortOrder = 1 },
                        new ExerciseTemplate { Name = "Remo Bajo", TargetConfig = "15-15-12-10", SortOrder = 2 },
                        new ExerciseTemplate { Name = "Serrucho", TargetConfig = "10-10-10-10", SortOrder = 3 },
                        new ExerciseTemplate { Name = "Pull-over", TargetConfig = "10-10-10-10", SortOrder = 4 },
                        new ExerciseTemplate { Name = "Curl con Barra", TargetConfig = "15-15-12-12", SortOrder = 5 },
                        new ExerciseTemplate { Name = "Alternado Mancuerna", TargetConfig = "15-15-12-12", SortOrder = 6 }
                    }
                },
                // Day 3: Legs
                new RoutineDay
                {
                    Name = "Piernas",
                    SortOrder = 3,
                    Exercises = new List<ExerciseTemplate>
                    {
                        new ExerciseTemplate { Name = "Prensa 45", TargetConfig = "15-12-10-10", SortOrder = 1 },
                        new ExerciseTemplate { Name = "Hack", TargetConfig = "15-12-10-10", SortOrder = 2 },
                        new ExerciseTemplate { Name = "Extension Cuadriceps", TargetConfig = "15-12-10-10", SortOrder = 3 },
                        new ExerciseTemplate { Name = "Curl Femoral", TargetConfig = "15-12-10-10", SortOrder = 4 },
                        new ExerciseTemplate { Name = "Gemelos", TargetConfig = "15-15-15-15", SortOrder = 5 }
                    }
                }
            }
        };
    }
}
