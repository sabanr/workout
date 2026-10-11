namespace IronTracker.Theme;

/// <summary>Helpers for composing Forge CSS class names.</summary>
public static class ForgeCss
{
    private const int MaxStagger = 8;

    /// <summary>Returns the entrance-delay class (stagger-1..8) for a zero-based list index.</summary>
    public static string Stagger(int index)
    {
        return $"stagger-{Math.Min(index + 1, MaxStagger)}";
    }
}
