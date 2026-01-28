namespace IronTracker.Models;

/// <summary>
/// Represents an exercise template within a routine day.
/// Contains the target configuration (e.g., "15-12-10-8" for rep targets per set).
/// </summary>
public class ExerciseTemplate
{
    public int Id { get; set; }
    
    public int RoutineDayId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Target configuration string defining rep goals per set.
    /// Format: "15-12-10-8" means 4 sets with 15, 12, 10, and 8 reps respectively.
    /// </summary>
    public string TargetConfig { get; set; } = string.Empty;
    
    /// <summary>
    /// Target weights string defining weight goals per set (in lbs).
    /// Format: "20-25-30-35" means weights for each corresponding set.
    /// Should have the same number of entries as TargetConfig.
    /// </summary>
    public string TargetWeights { get; set; } = string.Empty;
    
    public int SortOrder { get; set; }
    
    // Navigation property
    public RoutineDay RoutineDay { get; set; } = null!;
    
    /// <summary>
    /// Parses the TargetConfig string and returns the target reps for each set.
    /// </summary>
    public int[] GetTargetReps()
    {
        if (string.IsNullOrWhiteSpace(TargetConfig))
            return Array.Empty<int>();
            
        return TargetConfig
            .Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var reps) ? reps : 0)
            .ToArray();
    }
    
    /// <summary>
    /// Parses the TargetWeights string and returns the target weights for each set.
    /// </summary>
    public decimal[] GetTargetWeights()
    {
        if (string.IsNullOrWhiteSpace(TargetWeights))
            return Array.Empty<decimal>();
            
        return TargetWeights
            .Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => decimal.TryParse(s.Trim(), out var weight) ? weight : 0)
            .ToArray();
    }
    
    /// <summary>
    /// Gets the number of sets based on the TargetConfig.
    /// </summary>
    public int SetCount => GetTargetReps().Length;
}
