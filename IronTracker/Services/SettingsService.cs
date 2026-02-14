using IronTracker.Models;
using System.Globalization;

namespace IronTracker.Services;

/// <summary>
/// Manages application settings with persistence using MAUI Preferences.
/// </summary>
public class SettingsService
{
    private const string WeightUnitKey = "WeightUnit";
    private const string IsDarkModeKey = "IsDarkMode";
    private const string WindowWidthKey = "WindowWidth";
    private const string WindowHeightKey = "WindowHeight";
    private const string WindowXKey = "WindowX";
    private const string WindowYKey = "WindowY";
    private const string LanguageCodeKey = "LanguageCode";

    /// <summary>
    /// Event that fires when the weight unit preference changes.
    /// </summary>
    public event Action? WeightUnitChanged;

    /// <summary>
    /// Event that fires when any dashboard visibility setting changes.
    /// </summary>
    public event Action? DashboardVisibilityChanged;

    /// <summary>
    /// Event that fires when the language changes.
    /// </summary>
    public event Action? LanguageChanged;

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
    /// Gets or sets the window width.
    /// Defaults to 1200 if not set.
    /// </summary>
    public double WindowWidth
    {
        get => Preferences.Get(WindowWidthKey, 1200.0);
        set => Preferences.Set(WindowWidthKey, value);
    }

    /// <summary>
    /// Gets or sets the window height.
    /// Defaults to 800 if not set.
    /// </summary>
    public double WindowHeight
    {
        get => Preferences.Get(WindowHeightKey, 800.0);
        set => Preferences.Set(WindowHeightKey, value);
    }

    /// <summary>
    /// Gets or sets the window X position.
    /// Returns -1 if not set (indicates no saved position).
    /// </summary>
    public double WindowX
    {
        get => Preferences.Get(WindowXKey, -1.0);
        set => Preferences.Set(WindowXKey, value);
    }

    /// <summary>
    /// Gets or sets the window Y position.
    /// Returns -1 if not set (indicates no saved position).
    /// </summary>
    public double WindowY
    {
        get => Preferences.Get(WindowYKey, -1.0);
        set => Preferences.Set(WindowYKey, value);
    }

    /// <summary>
    /// Gets or sets whether the consecutive days streak card is visible on the dashboard.
    /// Defaults to true if not set.
    /// </summary>
    public bool ShowStreakCard
    {
        get => Preferences.Get(nameof(ShowStreakCard), true);
        set
        {
            Preferences.Set(nameof(ShowStreakCard), value);
            DashboardVisibilityChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets whether the quick start card is visible on the dashboard.
    /// Defaults to true if not set.
    /// </summary>
    public bool ShowQuickStartCard
    {
        get => Preferences.Get(nameof(ShowQuickStartCard), true);
        set
        {
            Preferences.Set(nameof(ShowQuickStartCard), value);
            DashboardVisibilityChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets whether the total recent sessions card is visible on the dashboard.
    /// Defaults to true if not set.
    /// </summary>
    public bool ShowTotalSessionsCard
    {
        get => Preferences.Get(nameof(ShowTotalSessionsCard), true);
        set
        {
            Preferences.Set(nameof(ShowTotalSessionsCard), value);
            DashboardVisibilityChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets whether the daily volume chart is visible on the dashboard.
    /// Defaults to true if not set.
    /// </summary>
    public bool ShowVolumeChart
    {
        get => Preferences.Get(nameof(ShowVolumeChart), true);
        set
        {
            Preferences.Set(nameof(ShowVolumeChart), value);
            DashboardVisibilityChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets whether the recent activity list is visible on the dashboard.
    /// Defaults to true if not set.
    /// </summary>
    public bool ShowRecentActivity
    {
        get => Preferences.Get(nameof(ShowRecentActivity), true);
        set
        {
            Preferences.Set(nameof(ShowRecentActivity), value);
            DashboardVisibilityChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets the current UI language code.
    /// Defaults to en-US if not set.
    /// </summary>
    public string LanguageCode
    {
        get => Preferences.Get(LanguageCodeKey, "en-US");
        set
        {
            var currentValue = LanguageCode;
            if (string.Equals(currentValue, value, StringComparison.OrdinalIgnoreCase))
                return;

            Preferences.Set(LanguageCodeKey, value);
            ApplyCulture(value);
            LanguageChanged?.Invoke();
        }
    }

    /// <summary>
    /// Initializes the current UI culture from saved settings.
    /// </summary>
    public void InitializeCulture()
    {
        ApplyCulture(LanguageCode);
    }

    /// <summary>
    /// Toggles between Lbs and Kg weight units.
    /// </summary>
    public void ToggleWeightUnit()
    {
        WeightUnit = WeightUnit == WeightUnit.Lbs ? WeightUnit.Kg : WeightUnit.Lbs;
    }

    /// <summary>
    /// Toggles between English and Spanish (Argentina).
    /// </summary>
    public void ToggleLanguage()
    {
        LanguageCode = string.Equals(LanguageCode, "es-AR", StringComparison.OrdinalIgnoreCase) ? "en-US" : "es-AR";
    }

    private static void ApplyCulture(string languageCode)
    {
        var culture = new CultureInfo(languageCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
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
