using IronTracker.Models;

namespace IronTracker.Services.Interfaces;

/// <summary>
/// Repository interface for workout data access operations.
/// </summary>
public interface IWorkoutRepository
{
    /// <summary>
    /// Gets all routines.
    /// </summary>
    Task<List<Routine>> GetRoutinesAsync();

    /// <summary>
    /// Gets a routine by ID with all its days and exercises.
    /// </summary>
    Task<Routine?> GetRoutineWithDaysAsync(int routineId);

    /// <summary>
    /// Gets a routine day with all its exercises.
    /// </summary>
    Task<RoutineDay?> GetRoutineDayWithExercisesAsync(int dayId);

    /// <summary>
    /// Gets workout session history within a date range.
    /// </summary>
    Task<List<WorkoutSession>> GetSessionHistoryAsync(DateTime from, DateTime to);

    /// <summary>
    /// Gets all set logs for a specific session.
    /// </summary>
    Task<List<SetLog>> GetSetLogsForSessionAsync(int sessionId);

    /// <summary>
    /// Gets the most recent sessions (for dashboard display).
    /// </summary>
    Task<List<WorkoutSession>> GetRecentSessionsAsync(int count = 10);

    /// <summary>
    /// Calculates the weekly volume (sum of reps x weight) for reporting.
    /// </summary>
    Task<Dictionary<DateTime, decimal>> GetWeeklyVolumeAsync(int weeksBack = 5);

    /// <summary>
    /// Counts consecutive workout days (streak).
    /// </summary>
    Task<int> GetConsecutiveDaysStreakAsync();

    // CRUD operations for Routines
    
    /// <summary>
    /// Creates a new routine.
    /// </summary>
    Task<Routine> CreateRoutineAsync(Routine routine);

    /// <summary>
    /// Updates an existing routine.
    /// </summary>
    Task<Routine> UpdateRoutineAsync(Routine routine);

    /// <summary>
    /// Deletes a routine by ID.
    /// </summary>
    Task DeleteRoutineAsync(int routineId);

    // CRUD operations for Routine Days

    /// <summary>
    /// Adds a day to a routine.
    /// </summary>
    Task<RoutineDay> AddRoutineDayAsync(RoutineDay day);

    /// <summary>
    /// Updates a routine day.
    /// </summary>
    Task<RoutineDay> UpdateRoutineDayAsync(RoutineDay day);

    /// <summary>
    /// Deletes a routine day.
    /// </summary>
    Task DeleteRoutineDayAsync(int dayId);

    // CRUD operations for Exercises

    /// <summary>
    /// Adds an exercise to a routine day.
    /// </summary>
    Task<ExerciseTemplate> AddExerciseAsync(ExerciseTemplate exercise);

    /// <summary>
    /// Updates an exercise.
    /// </summary>
    Task<ExerciseTemplate> UpdateExerciseAsync(ExerciseTemplate exercise);

    /// <summary>
    /// Deletes an exercise.
    /// </summary>
    Task DeleteExerciseAsync(int exerciseId);

    /// <summary>
    /// Gets the last used weights for an exercise by name.
    /// Returns a dictionary of SetNumber -> Weight.
    /// </summary>
    Task<Dictionary<int, decimal>> GetLastWeightsForExerciseAsync(string exerciseName);
}
