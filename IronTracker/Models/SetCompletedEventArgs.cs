namespace IronTracker.Models;

/// <summary>
/// Event arguments for when a set is completed during a workout session.
/// </summary>
public class SetCompletedEventArgs
{
    public int SessionId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal Weight { get; set; }
}
