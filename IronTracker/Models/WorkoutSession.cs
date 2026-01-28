namespace IronTracker.Models;

/// <summary>
/// Represents an active or completed workout session.
/// </summary>
public class WorkoutSession
{
    public int Id { get; set; }
    
    public int RoutineDayId { get; set; }
    
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    
    public DateTime? EndTime { get; set; }
    
    public string? Notes { get; set; }
    
    // Navigation properties
    public RoutineDay RoutineDay { get; set; } = null!;
    
    public ICollection<SetLog> SetLogs { get; set; } = new List<SetLog>();
    
    /// <summary>
    /// Gets the duration of the session.
    /// </summary>
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
    
    /// <summary>
    /// Indicates if the session is currently active (not ended).
    /// </summary>
    public bool IsActive => !EndTime.HasValue;
    
    /// <summary>
    /// Calculates the total volume (sum of reps x weight) for this session.
    /// </summary>
    public decimal TotalVolume => SetLogs.Sum(s => s.RepsPerformed * s.WeightUsed);
}
