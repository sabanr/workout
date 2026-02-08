using Microsoft.EntityFrameworkCore;
using IronTracker.Data;
using IronTracker.Models;
using IronTracker.Services.Interfaces;

namespace IronTracker.Services;

/// <summary>
/// Repository implementation for workout data access using EF Core.
/// </summary>
public class WorkoutRepository : IWorkoutRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public WorkoutRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Routine>> GetRoutinesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Routines
            .Include(r => r.Days)
                .ThenInclude(d => d.Exercises)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Routine?> GetRoutineWithDaysAsync(int routineId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Routines
            .Include(r => r.Days)
                .ThenInclude(d => d.Exercises)
            .FirstOrDefaultAsync(r => r.Id == routineId);
    }

    public async Task<RoutineDay?> GetRoutineDayWithExercisesAsync(int dayId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.RoutineDays
            .Include(d => d.Exercises.OrderBy(e => e.SortOrder))
            .Include(d => d.Routine)
            .FirstOrDefaultAsync(d => d.Id == dayId);
    }

    public async Task<List<WorkoutSession>> GetSessionHistoryAsync(DateTime from, DateTime to)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.WorkoutSessions
            .Include(s => s.RoutineDay)
            .Include(s => s.SetLogs)
            .Where(s => s.StartTime >= from && s.StartTime <= to)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<List<SetLog>> GetSetLogsForSessionAsync(int sessionId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SetLogs
            .Where(l => l.WorkoutSessionId == sessionId)
            .OrderBy(l => l.ExerciseName)
            .ThenBy(l => l.SetNumber)
            .ToListAsync();
    }

    public async Task<List<WorkoutSession>> GetRecentSessionsAsync(int count = 10)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.WorkoutSessions
            .Include(s => s.RoutineDay)
            .Include(s => s.SetLogs)
            .Where(s => s.EndTime != null)
            .OrderByDescending(s => s.StartTime)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Dictionary<DateTime, decimal>> GetWeeklyVolumeAsync(int weeksBack = 5)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var startDate = DateTime.Now.Date.AddDays(-(weeksBack * 7));
        
        var logs = await context.SetLogs
            .Where(l => l.CompletedAt >= startDate)
            .ToListAsync();

        // Group by week start (Monday) using local time
        var weeklyVolume = logs
            .GroupBy(l => GetWeekStart(l.CompletedAt.ToLocalTime()))
            .ToDictionary(
                g => g.Key,
                g => g.Sum(l => l.RepsPerformed * l.WeightUsed)
            );

        return weeklyVolume;
    }

    public async Task<int> GetConsecutiveDaysStreakAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var sessions = await context.WorkoutSessions
            .Where(s => s.EndTime != null)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        if (sessions.Count == 0)
            return 0;

        // Convert to local dates for streak calculation
        var localDates = sessions
            .Select(s => s.StartTime.ToLocalTime().Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        var streak = 0;
        var expectedDate = DateTime.Now.Date;

        // If no session today, check if there was one yesterday
        if (localDates.First() != expectedDate)
        {
            expectedDate = expectedDate.AddDays(-1);
            if (localDates.First() != expectedDate)
                return 0;
        }

        foreach (var sessionDate in localDates)
        {
            if (sessionDate == expectedDate)
            {
                streak++;
                expectedDate = expectedDate.AddDays(-1);
            }
            else if (sessionDate < expectedDate)
            {
                break;
            }
        }

        return streak;
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    // CRUD operations for Routines

    public async Task<Routine> CreateRoutineAsync(Routine routine)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Routines.Add(routine);
        await context.SaveChangesAsync();
        return routine;
    }

    public async Task<Routine> UpdateRoutineAsync(Routine routine)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Routines.Update(routine);
        await context.SaveChangesAsync();
        return routine;
    }

    public async Task DeleteRoutineAsync(int routineId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var routine = await context.Routines.FindAsync(routineId);
        if (routine != null)
        {
            context.Routines.Remove(routine);
            await context.SaveChangesAsync();
        }
    }

    // CRUD operations for Routine Days

    public async Task<RoutineDay> AddRoutineDayAsync(RoutineDay day)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.RoutineDays.Add(day);
        await context.SaveChangesAsync();
        return day;
    }

    public async Task<RoutineDay> UpdateRoutineDayAsync(RoutineDay day)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.RoutineDays.Update(day);
        await context.SaveChangesAsync();
        return day;
    }

    public async Task DeleteRoutineDayAsync(int dayId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var day = await context.RoutineDays.FindAsync(dayId);
        if (day != null)
        {
            context.RoutineDays.Remove(day);
            await context.SaveChangesAsync();
        }
    }

    // CRUD operations for Exercises

    public async Task<ExerciseTemplate> AddExerciseAsync(ExerciseTemplate exercise)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.ExerciseTemplates.Add(exercise);
        await context.SaveChangesAsync();
        return exercise;
    }

    public async Task<ExerciseTemplate> UpdateExerciseAsync(ExerciseTemplate exercise)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.ExerciseTemplates.Update(exercise);
        await context.SaveChangesAsync();
        return exercise;
    }

    public async Task DeleteExerciseAsync(int exerciseId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var exercise = await context.ExerciseTemplates.FindAsync(exerciseId);
        if (exercise != null)
        {
            context.ExerciseTemplates.Remove(exercise);
            await context.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<int, decimal>> GetLastWeightsForExerciseAsync(string exerciseName)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Get the most recent completed session that has this exercise
        var recentLogs = await context.SetLogs
            .Where(l => l.ExerciseName == exerciseName)
            .OrderByDescending(l => l.CompletedAt)
            .Take(10) // Get last 10 sets for this exercise
            .ToListAsync();

        if (recentLogs.Count == 0)
            return new Dictionary<int, decimal>();

        // Get the session ID of the most recent log
        var mostRecentSessionId = recentLogs.First().WorkoutSessionId;

        // Return weights from that session, grouped by set number
        return recentLogs
            .Where(l => l.WorkoutSessionId == mostRecentSessionId)
            .ToDictionary(l => l.SetNumber, l => l.WeightUsed);
    }
}
