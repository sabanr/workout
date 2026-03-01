using IronTracker.Models;
using IronTracker.Services.Interfaces;

namespace IronTracker.Services;

/// <summary>
/// High-level workout service that combines repository and session management.
/// </summary>
public class WorkoutService
{
    private readonly IWorkoutRepository _repository;
    private readonly ISessionManager _sessionManager;

    public WorkoutService(IWorkoutRepository repository, ISessionManager sessionManager)
    {
        _repository = repository;
        _sessionManager = sessionManager;
    }

    /// <summary>
    /// Saves a completed set during an active session.
    /// </summary>
    public async Task<SetLog> SaveSetAsync(
        int sessionId,
        string exerciseName,
        int setNumber,
        int reps,
        decimal weight)
    {
        var setLog = new SetLog
        {
            WorkoutSessionId = sessionId,
            ExerciseName = exerciseName,
            SetNumber = setNumber,
            RepsPerformed = reps,
            WeightUsed = weight,
            CompletedAt = DateTime.UtcNow
        };

        return await _sessionManager.SaveSetAsync(setLog);
    }

    /// <summary>
    /// Gets dashboard statistics.
    /// </summary>
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var streak = await _repository.GetConsecutiveDaysStreakAsync();
        var weeklyVolume = await _repository.GetWeeklyVolumeAsync(5);
        var recentSessions = await _repository.GetRecentSessionsAsync(5);

        // Calculate last 30 days stats (rolling window)
        var now = DateTime.Now;
        var startOfPeriod = now.Date.AddDays(-30);
        var endOfPeriod = now.Date.AddDays(1).AddTicks(-1); // End of today
        
        // Use UTC range for database query
        var startUtc = startOfPeriod.ToUniversalTime();
        var endUtc = endOfPeriod.ToUniversalTime();
        
        var periodSessions = await _repository.GetSessionHistoryAsync(startUtc, endUtc);
        
        // Count all sessions started in this period
        var workoutsThisMonth = periodSessions.Count;
        
        // Volume can come from any session that has logs (even if not finished yet)
        var volumeThisMonth = periodSessions.Sum(s => s.TotalVolume);
        
        // Duration only makes sense for completed sessions
        var durations = periodSessions
            .Where(s => s.EndTime != null && s.Duration.HasValue)
            .Select(s => s.Duration!.Value.TotalMinutes)
            .ToList();
            
        var avgDuration = durations.Any() 
            ? TimeSpan.FromMinutes(durations.Average()) 
            : TimeSpan.Zero;

        var favoriteRoutine = periodSessions
            .Where(s => s.RoutineDay != null)
            .GroupBy(s => s.RoutineDay.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        // Calculate training frequency percentage (unique days with sessions / 30)
        var uniqueDaysCount = periodSessions
            .Select(s => s.StartTime.ToLocalTime().Date)
            .Distinct()
            .Count();
        var frequency = (double)uniqueDaysCount / 30.0 * 100.0;

        return new DashboardStats
        {
            ConsecutiveDaysStreak = streak,
            WeeklyVolume = weeklyVolume,
            RecentSessions = recentSessions,
            WorkoutsThisMonth = workoutsThisMonth,
            VolumeThisMonth = volumeThisMonth,
            AverageDuration = avgDuration,
            FavoriteRoutineName = favoriteRoutine,
            TrainingFrequency = frequency
        };
    }

    /// <summary>
    /// Gets or creates an active session for a routine day.
    /// </summary>
    public async Task<WorkoutSession> GetOrStartSessionAsync(int routineDayId)
    {
        var activeSession = await _sessionManager.GetActiveSessionAsync();
        
        if (activeSession != null && activeSession.RoutineDayId == routineDayId)
            return activeSession;

        return await _sessionManager.StartSessionAsync(routineDayId);
    }

    /// <summary>
    /// Gets all routines.
    /// </summary>
    public Task<List<Routine>> GetRoutinesAsync() => _repository.GetRoutinesAsync();

    /// <summary>
    /// Gets a routine day with exercises.
    /// </summary>
    public Task<RoutineDay?> GetRoutineDayAsync(int dayId) => _repository.GetRoutineDayWithExercisesAsync(dayId);

    /// <summary>
    /// Gets the active session.
    /// </summary>
    public Task<WorkoutSession?> GetActiveSessionAsync() => _sessionManager.GetActiveSessionAsync();

    /// <summary>
    /// Ends the active session.
    /// </summary>
    public Task<WorkoutSession?> EndSessionAsync() => _sessionManager.EndSessionAsync();

    /// <summary>
    /// Cancels the active session without saving.
    /// </summary>
    public Task<WorkoutSession?> CancelSessionAsync() => _sessionManager.CancelSessionAsync();

    /// <summary>
    /// Checks if there's an active session.
    /// </summary>
    public Task<bool> HasActiveSessionAsync() => _sessionManager.HasActiveSessionAsync();

    /// <summary>
    /// Gets set logs for an exercise in a session.
    /// </summary>
    public Task<List<SetLog>> GetExerciseLogsAsync(int sessionId, string exerciseName) 
        => _sessionManager.GetExerciseLogsAsync(sessionId, exerciseName);

    /// <summary>
    /// Gets workout session history within a date range.
    /// </summary>
    public Task<List<WorkoutSession>> GetSessionHistoryAsync(DateTime from, DateTime to) 
        => _repository.GetSessionHistoryAsync(from, to);

    // Routine CRUD operations

    /// <summary>
    /// Gets a routine by ID with all days and exercises.
    /// </summary>
    public Task<Routine?> GetRoutineAsync(int routineId) => _repository.GetRoutineWithDaysAsync(routineId);

    /// <summary>
    /// Creates a new routine.
    /// </summary>
    public Task<Routine> CreateRoutineAsync(Routine routine) => _repository.CreateRoutineAsync(routine);

    /// <summary>
    /// Updates an existing routine.
    /// </summary>
    public Task<Routine> UpdateRoutineAsync(Routine routine) => _repository.UpdateRoutineAsync(routine);

    /// <summary>
    /// Deletes a routine.
    /// </summary>
    public Task DeleteRoutineAsync(int routineId) => _repository.DeleteRoutineAsync(routineId);

    // Routine Day CRUD operations

    /// <summary>
    /// Adds a day to a routine.
    /// </summary>
    public Task<RoutineDay> AddRoutineDayAsync(RoutineDay day) => _repository.AddRoutineDayAsync(day);

    /// <summary>
    /// Updates a routine day.
    /// </summary>
    public Task<RoutineDay> UpdateRoutineDayAsync(RoutineDay day) => _repository.UpdateRoutineDayAsync(day);

    /// <summary>
    /// Deletes a routine day.
    /// </summary>
    public Task DeleteRoutineDayAsync(int dayId) => _repository.DeleteRoutineDayAsync(dayId);

    // Exercise CRUD operations

    /// <summary>
    /// Adds an exercise to a routine day.
    /// </summary>
    public Task<ExerciseTemplate> AddExerciseAsync(ExerciseTemplate exercise) => _repository.AddExerciseAsync(exercise);

    /// <summary>
    /// Updates an exercise.
    /// </summary>
    public Task<ExerciseTemplate> UpdateExerciseAsync(ExerciseTemplate exercise) => _repository.UpdateExerciseAsync(exercise);

    /// <summary>
    /// Deletes an exercise.
    /// </summary>
    public Task DeleteExerciseAsync(int exerciseId) => _repository.DeleteExerciseAsync(exerciseId);

    /// <summary>
    /// Gets the last used weights for an exercise.
    /// </summary>
    public Task<Dictionary<int, decimal>> GetLastWeightsForExerciseAsync(string exerciseName) 
        => _repository.GetLastWeightsForExerciseAsync(exerciseName);
}

/// <summary>
/// Dashboard statistics data transfer object.
/// </summary>
public class DashboardStats
{
    public int ConsecutiveDaysStreak { get; set; }
    public Dictionary<DateTime, decimal> WeeklyVolume { get; set; } = new();
    public List<WorkoutSession> RecentSessions { get; set; } = new();
    
    // New monthly metrics
    public int WorkoutsThisMonth { get; set; }
    public decimal VolumeThisMonth { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public string? FavoriteRoutineName { get; set; }
    public double TrainingFrequency { get; set; }
}
