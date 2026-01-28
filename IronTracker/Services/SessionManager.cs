using Microsoft.EntityFrameworkCore;
using IronTracker.Data;
using IronTracker.Models;
using IronTracker.Services.Interfaces;

namespace IronTracker.Services;

/// <summary>
/// Manages active workout sessions.
/// </summary>
public class SessionManager : ISessionManager
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public SessionManager(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<WorkoutSession> StartSessionAsync(int routineDayId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // End any existing active sessions
        var activeSessions = await context.WorkoutSessions
            .Where(s => s.EndTime == null)
            .ToListAsync();

        foreach (var session in activeSessions)
        {
            session.EndTime = DateTime.UtcNow;
        }

        // Create new session
        var newSession = new WorkoutSession
        {
            RoutineDayId = routineDayId,
            StartTime = DateTime.UtcNow
        };

        context.WorkoutSessions.Add(newSession);
        await context.SaveChangesAsync();

        // Reload with navigation properties
        return await context.WorkoutSessions
            .Include(s => s.RoutineDay)
                .ThenInclude(d => d.Exercises.OrderBy(e => e.SortOrder))
            .FirstAsync(s => s.Id == newSession.Id);
    }

    public async Task<WorkoutSession?> GetActiveSessionAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.WorkoutSessions
            .Include(s => s.RoutineDay)
                .ThenInclude(d => d.Exercises.OrderBy(e => e.SortOrder))
            .Include(s => s.SetLogs)
            .FirstOrDefaultAsync(s => s.EndTime == null);
    }

    public async Task<WorkoutSession?> EndSessionAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var activeSession = await context.WorkoutSessions
            .Include(s => s.SetLogs)
            .FirstOrDefaultAsync(s => s.EndTime == null);

        if (activeSession == null)
            return null;

        activeSession.EndTime = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return activeSession;
    }

    public async Task<SetLog> SaveSetAsync(SetLog setLog)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        if (setLog.Id == 0)
        {
            // New set log
            setLog.CompletedAt = DateTime.UtcNow;
            context.SetLogs.Add(setLog);
        }
        else
        {
            // Update existing
            var existing = await context.SetLogs.FindAsync(setLog.Id);
            if (existing != null)
            {
                existing.RepsPerformed = setLog.RepsPerformed;
                existing.WeightUsed = setLog.WeightUsed;
            }
        }

        await context.SaveChangesAsync();
        return setLog;
    }

    public async Task<List<SetLog>> GetExerciseLogsAsync(int sessionId, string exerciseName)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SetLogs
            .Where(l => l.WorkoutSessionId == sessionId && l.ExerciseName == exerciseName)
            .OrderBy(l => l.SetNumber)
            .ToListAsync();
    }

    public async Task DeleteSetAsync(int setLogId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var setLog = await context.SetLogs.FindAsync(setLogId);
        if (setLog != null)
        {
            context.SetLogs.Remove(setLog);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasActiveSessionAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.WorkoutSessions.AnyAsync(s => s.EndTime == null);
    }
}
