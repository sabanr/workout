using IronTracker.Models;

namespace IronTracker.Services;

/// <summary>
/// Manages application settings with persistence using MAUI Preferences.
/// </summary>
public class SettingsService
{
    private const string WeightUnitKey = "WeightUnit";
    private const string IsDarkModeKey = "IsDarkMode";

    /// <summary>
    /// Event that fires when the weight unit preference changes.
    /// </summary>
    public event Action? WeightUnitChanged;

    /// <summary>
    /// Gets or sets the current weight unit preference.
    /// Defaults to Lbs if not set.
    /// </summary>
    public WeightUnit WeightUnit
    {
        get => (WeightUnit)Preferences.Get(WeightUnitKey, (int)WeightUnit.Lbs);
        set
        {
            var currentValue = WeightUnit;
            if (currentValue != value)
            {
                Preferences.Set(WeightUnitKey, (int)value);
                WeightUnitChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether dark mode is enabled.
    /// Defaults to true if not set.
    /// </summary>
    public bool IsDarkMode
    {
        get => Preferences.Get(IsDarkModeKey, true);
        set => Preferences.Set(IsDarkModeKey, value);
    }

    /// <summary>
    /// Toggles between Lbs and Kg weight units.
    /// </summary>
    public void ToggleWeightUnit()
    {
        WeightUnit = WeightUnit == WeightUnit.Lbs ? WeightUnit.Kg : WeightUnit.Lbs;
    }

    /// <summary>
    /// Converts a weight value from pounds to the current display unit.
    /// </summary>
    /// <param name="weightInLbs">The weight in pounds</param>
    /// <returns>The weight in the current display unit</returns>
    public decimal ConvertToDisplayUnit(decimal weightInLbs)
    {
        if (WeightUnit == WeightUnit.Lbs)
            return weightInLbs;

        // Convert lbs to kg: divide by 2.20462
        return Math.Round(weightInLbs / 2.20462m, 1);
    }

    /// <summary>
    /// Converts a weight value from the current display unit to pounds (for storage).
    /// </summary>
    /// <param name="weightInDisplayUnit">The weight in the current display unit</param>
    /// <returns>The weight in pounds</returns>
    public decimal ConvertToStorageUnit(decimal weightInDisplayUnit)
    {
        if (WeightUnit == WeightUnit.Lbs)
            return weightInDisplayUnit;

        // Convert kg to lbs: multiply by 2.20462
        return Math.Round(weightInDisplayUnit * 2.20462m, 2);
    }

    /// <summary>
    /// Gets the display unit abbreviation ("lbs" or "kg").
    /// </summary>
    public string GetUnitAbbreviation() => WeightUnit == WeightUnit.Lbs ? "lbs" : "kg";

    /// <summary>
    /// Gets the appropriate step size for weight input fields based on the current unit.
    /// </summary>
    public decimal GetWeightStepSize() => WeightUnit == WeightUnit.Lbs ? 5m : 2.5m;

    /// <summary>
    /// Gets the appropriate minimum weight value based on the current unit.
    /// </summary>
    public decimal GetMinWeight() => WeightUnit == WeightUnit.Lbs ? 0m : 0m;

    /// <summary>
    /// Gets the appropriate maximum weight value based on the current unit.
    /// </summary>
    public decimal GetMaxWeight() => WeightUnit == WeightUnit.Lbs ? 1000m : 454m; // ~1000 lbs ≈ 454 kg

    /// <summary>
    /// Gets the weight increment text for helper text.
    /// </summary>
    public string GetWeightIncrementText() => $"{GetWeightStepSize()}{GetUnitAbbreviation()} increments";
}
