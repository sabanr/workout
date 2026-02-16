using IronTracker.Models;

namespace IronTracker.Services.Interfaces;

/// <summary>
/// Interface for managing active workout sessions.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Starts a new workout session for the specified routine day.
    /// </summary>
    Task<WorkoutSession> StartSessionAsync(int routineDayId);

    /// <summary>
    /// Gets the currently active session (if any).
    /// </summary>
    Task<WorkoutSession?> GetActiveSessionAsync();

    /// <summary>
    /// Ends the currently active session.
    /// </summary>
    Task<WorkoutSession?> EndSessionAsync();

    /// <summary>
    /// Cancels the currently active session and deletes all associated data.
    /// </summary>
    Task<WorkoutSession?> CancelSessionAsync();

    /// <summary>
    /// Saves a set log for the active session.
    /// </summary>
    Task<SetLog> SaveSetAsync(SetLog setLog);

    /// <summary>
    /// Gets all set logs for a specific exercise in the active session.
    /// </summary>
    Task<List<SetLog>> GetExerciseLogsAsync(int sessionId, string exerciseName);

    /// <summary>
    /// Deletes a set log.
    /// </summary>
    Task DeleteSetAsync(int setLogId);

    /// <summary>
    /// Checks if there's an active session.
    /// </summary>
    Task<bool> HasActiveSessionAsync();
}
