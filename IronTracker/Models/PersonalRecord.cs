namespace IronTracker.Models;

/// <summary>
/// Represents a personal record (highest weight) for an exercise.
/// </summary>
public class PersonalRecord
{
    /// <summary>
    /// Name of the exercise.
    /// </summary>
    public string ExerciseName { get; set; } = string.Empty;

    /// <summary>
    /// Maximum weight lifted for this exercise.
    /// </summary>
    public decimal MaxWeight { get; set; }

    /// <summary>
    /// Date when this personal record was achieved.
    /// </summary>
    public DateTime AchievedAt { get; set; }

    /// <summary>
    /// Total number of sets performed for this exercise.
    /// </summary>
    public int SetCount { get; set; }
}
