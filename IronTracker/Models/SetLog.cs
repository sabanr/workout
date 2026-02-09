namespace IronTracker.Models;

/// <summary>
/// Represents a single set log entry within a workout session.
/// Denormalized to facilitate historical reporting even if exercises are renamed/deleted.
/// </summary>
public class SetLog
{
    public int Id { get; set; }
    
    public int WorkoutSessionId { get; set; }
    
    /// <summary>
    /// Exercise name stored as string for historical integrity.
    /// This allows reports to work correctly even if the original exercise is modified or deleted.
    /// </summary>
    public string ExerciseName { get; set; } = string.Empty;
    
    /// <summary>
    /// Set number within the exercise (1-based).
    /// </summary>
    public int SetNumber { get; set; }
    
    /// <summary>
    /// Number of repetitions performed.
    /// </summary>
    public int RepsPerformed { get; set; }
    
    /// <summary>
    /// Weight used in pounds (lbs).
    /// </summary>
    public decimal WeightUsed { get; set; }
    
    /// <summary>
    /// Timestamp when the set was completed.
    /// </summary>
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public WorkoutSession WorkoutSession { get; set; } = null!;
    
    /// <summary>
    /// Calculates the volume for this set (reps x weight).
    /// </summary>
    public decimal Volume => RepsPerformed * WeightUsed;
}
