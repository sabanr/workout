namespace IronTracker.Models;

/// <summary>
/// Represents a workout routine containing multiple training days.
/// </summary>
public class Routine
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public ICollection<RoutineDay> Days { get; set; } = new List<RoutineDay>();
}
