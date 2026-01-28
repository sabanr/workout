namespace IronTracker.Models;

/// <summary>
/// Represents a single training day within a routine (e.g., "Chest Day", "Leg Day").
/// </summary>
public class RoutineDay
{
    public int Id { get; set; }
    
    public int RoutineId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public int SortOrder { get; set; }
    
    // Navigation properties
    public Routine Routine { get; set; } = null!;
    
    public ICollection<ExerciseTemplate> Exercises { get; set; } = new List<ExerciseTemplate>();
    
    public ICollection<WorkoutSession> Sessions { get; set; } = new List<WorkoutSession>();
}
