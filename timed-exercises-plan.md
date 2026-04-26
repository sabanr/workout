# Timed Exercises Plan

## Problem

IronTracker currently assumes every exercise is tracked as sets with reps and weight.
This works for strength exercises, but cardio and other no-weight exercises need a
different shape: a target duration, a timer during the active workout, and history
that displays time instead of `0 lbs x 1 rep`.

The current workaround in the app data is:

- `Elliptical (lvl 3)` has `TargetConfig = 1` and `TargetWeights = 0`.
- Existing cardio logs are stored as `1 rep` with `0 weight`.
- Some recent cardio sessions have no set logs at all and rely only on session duration.

The goal is to make timed exercises first-class without breaking existing weighted
exercise tracking or historical data.

## Current Touch Points

- `IronTracker/Models/ExerciseTemplate.cs`
  - `TargetConfig` stores reps per set.
  - `TargetWeights` stores target weights per set.
  - `SetCount` is derived from `TargetConfig`.
- `IronTracker/Models/SetLog.cs`
  - Stores `RepsPerformed` and `WeightUsed`.
  - `Volume` is calculated as `reps * weight`.
- `IronTracker/Components/ExerciseDialog.razor`
  - Lets users configure reps and optional weights.
- `IronTracker/Components/SetTracker.razor`
  - Renders the set/reps/weight tracking table.
- `IronTracker/Components/RestTimer.razor`
  - Provides the existing countdown timer behavior and completion sound.
- `IronTracker/Components/Pages/ActiveSession.razor`
  - Selects exercises.
  - Shows `SetTracker`.
  - Starts a 60-second rest timer after each completed set.
- `IronTracker/Components/Pages/History.razor`
  - Displays best set and volume for each exercise.
- `IronTracker/Services/WorkoutRepository.cs`
  - Calculates volume, last weights, and personal records from `SetLogs`.

## Data Model

Add a tracking mode enum:

```csharp
namespace IronTracker.Models;

/// <summary>
/// Defines how an exercise is tracked during a workout session.
/// </summary>
public enum ExerciseTrackingMode
{
    WeightedSets = 0,
    RepsOnly = 1,
    Timed = 2
}
```

Update `ExerciseTemplate`:

```csharp
/// <summary>
/// Defines how this exercise should be tracked.
/// </summary>
public ExerciseTrackingMode TrackingMode { get; set; } = ExerciseTrackingMode.WeightedSets;

/// <summary>
/// Target duration in seconds for timed exercises.
/// </summary>
public int? TargetDurationSeconds { get; set; }
```

Update `SetLog`:

```csharp
/// <summary>
/// Tracking mode used when this log was recorded.
/// </summary>
public ExerciseTrackingMode TrackingMode { get; set; } = ExerciseTrackingMode.WeightedSets;

/// <summary>
/// Completed duration in seconds for timed exercises.
/// </summary>
public int? DurationSeconds { get; set; }
```

For timed exercises, save one log per completed timed exercise:

- `SetNumber = 1`
- `RepsPerformed = 0`
- `WeightUsed = 0`
- `TrackingMode = ExerciseTrackingMode.Timed`
- `DurationSeconds = target or actual completed duration`

This preserves the existing `SetLogs` table as the session activity log while adding
the missing duration concept.

## Database Upgrade

The app currently uses `EnsureCreatedAsync` and does not appear to have EF migrations.
Because existing app databases already exist, add an idempotent schema upgrade step
before seeding data.

Required columns:

```sql
ALTER TABLE ExerciseTemplates ADD COLUMN TrackingMode INTEGER NOT NULL DEFAULT 0;
ALTER TABLE ExerciseTemplates ADD COLUMN TargetDurationSeconds INTEGER NULL;
ALTER TABLE SetLogs ADD COLUMN TrackingMode INTEGER NOT NULL DEFAULT 0;
ALTER TABLE SetLogs ADD COLUMN DurationSeconds INTEGER NULL;
```

Implementation detail:

- Check whether each column exists using `PRAGMA table_info(...)`.
- Only run each `ALTER TABLE` when the column is missing.
- Keep default mode `0` so all existing exercises remain weighted by default.

Optional backfill:

```sql
UPDATE ExerciseTemplates
SET TrackingMode = 2,
    TargetDurationSeconds = 1200
WHERE TargetConfig = '1'
  AND TargetWeights = '0'
  AND Name LIKE '%Elliptical%';
```

Keep the backfill conservative. It is safer to let users edit ambiguous `0 weight`
exercises manually than to convert bodyweight exercises accidentally.

## Exercise Editing UI

Update `ExerciseDialog.razor` to support tracking modes.

For `WeightedSets`:

- Show the existing target reps field.
- Show the existing target weights field.
- Validate target weights only when provided.

For `RepsOnly`:

- Show target reps.
- Hide target weights.
- Save `TargetWeights` as an empty string.

For `Timed`:

- Hide target reps and target weights.
- Show target duration, preferably minutes and seconds or total minutes.
- Validate duration is greater than zero.
- Save `TargetDurationSeconds`.
- Use a harmless placeholder `TargetConfig` value if the existing database requires
  it to be non-empty, for example `"1"`.

Add localized labels:

- `TrackingMode`
- `WeightedSets`
- `RepsOnly`
- `Timed`
- `TargetDuration`
- `DurationRequired`

## Active Session UI

Keep `SetTracker` for `WeightedSets` and `RepsOnly`.

Changes in `SetTracker`:

- Hide the weight column for `RepsOnly`.
- Store `WeightUsed = 0` for reps-only logs.
- Avoid loading last weights for reps-only exercises.

Add a new component:

```text
IronTracker/Components/TimedExerciseTracker.razor
```

Responsibilities:

- Display the exercise name and target duration.
- Show a countdown timer initialized from `Exercise.TargetDurationSeconds`.
- Provide start, pause/resume, add 30 seconds, reset, and complete controls.
- Play the existing completion sound when the timer reaches zero.
- Raise an event when the timed exercise is completed.

Refactor option:

- Extract common timer logic from `RestTimer.razor` into a reusable timer component,
  or make `RestTimer` configurable enough for both rest timers and exercise timers.
- Keep labels and behavior distinct: rest timer completion should not save an exercise,
  while timed exercise completion should save a timed log.

Update `ActiveSession.razor`:

- Render `TimedExerciseTracker` when `Exercise.TrackingMode == Timed`.
- Render `SetTracker` otherwise.
- Do not start the 60-second rest timer after completing a timed exercise.
- Mark a timed exercise complete when at least one timed log exists for that exercise
  in the active session.

## Service Layer

Add a service method:

```csharp
/// <summary>
/// Saves a completed timed exercise during an active session.
/// </summary>
public async Task<SetLog> SaveTimedExerciseAsync(
    int sessionId,
    string exerciseName,
    int durationSeconds)
```

The method should create a `SetLog` using:

- `SetNumber = 1`
- `RepsPerformed = 0`
- `WeightUsed = 0`
- `TrackingMode = ExerciseTrackingMode.Timed`
- `DurationSeconds = durationSeconds`
- `CompletedAt = DateTime.UtcNow`

Update `SetCompletedEventArgs` or add a separate event args type for timed exercise
completion. A separate type is cleaner:

```csharp
public class TimedExerciseCompletedEventArgs
{
    public int SessionId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
}
```

## History

Update `History.razor` so grouped exercise logs render based on tracking mode.

For weighted logs:

- Show sets count.
- Show best set as weight x reps.
- Show volume.

For reps-only logs:

- Show sets count.
- Show total reps or best reps.
- Show no volume, or display `-`.

For timed logs:

- Show completed duration, for example `20m` or `20:00`.
- Show no weight.
- Show no volume, or display `-`.

Existing legacy cardio logs with `WeightUsed = 0`, `RepsPerformed = 1`, and no
`DurationSeconds` should continue to display without errors. If the parent session
duration is available and the exercise appears to be timed, the UI may display the
session duration as a fallback.

## Dashboard And Reporting

Volume should remain strength-only.

Update calculations so timed and reps-only logs do not create misleading strength
stats:

- `WorkoutSession.TotalVolume` should sum only logs with weighted tracking mode,
  or only logs where `WeightUsed > 0`.
- `GetWeeklyVolumeAsync` should exclude timed/reps-only logs.
- `GetTopPersonalRecordsAsync` should exclude timed/reps-only logs and zero-weight logs.
- `GetLastWeightsForExerciseAsync` should only be called or return values for
  weighted exercises.

Cardio sessions should still count toward:

- Workouts completed
- Streak
- Training frequency
- Recent activity
- Average session duration

## Localization

Update both resource files:

- `IronTracker/Resources/Localization/AppResources.resx`
- `IronTracker/Resources/Localization/AppResources.es-AR.resx`

Suggested English keys:

- `TrackingMode`: `Tracking Mode`
- `WeightedSets`: `Weighted Sets`
- `RepsOnly`: `Reps Only`
- `Timed`: `Timed`
- `TargetDuration`: `Target Duration`
- `DurationMinutes`: `Minutes`
- `DurationSeconds`: `Seconds`
- `DurationRequired`: `Duration is required`
- `StartTimer`: `Start Timer`
- `PauseTimer`: `Pause`
- `ResumeTimer`: `Resume`
- `CompleteExercise`: `Complete Exercise`
- `TimedExerciseCompleted`: `Timed exercise completed`

## Compatibility Notes

- Do not remove or reinterpret existing `TargetConfig` and `TargetWeights` yet.
- Keep existing weighted exercise behavior unchanged.
- Treat `TrackingMode = 0` as weighted for all old records.
- Avoid broad automatic conversion of zero-weight exercises because bodyweight and
  machine exercises may legitimately have no target weights.
- Consider a later cleanup migration only after the timed model has been used in
  production data.

## Test Plan

Manual tests:

1. Create a weighted exercise and complete sets as before.
2. Create a reps-only exercise and confirm the weight column is hidden.
3. Create a timed exercise with a 20-minute target.
4. Start a timed workout and confirm the timer starts from the configured duration.
5. Complete the timed exercise and confirm one timed log is saved.
6. Confirm no 60-second rest timer starts after a timed exercise completes.
7. Confirm history shows timed duration instead of `0 lbs x 0`.
8. Confirm dashboard volume and personal records ignore timed exercise logs.
9. Confirm legacy cardio data still renders without errors.

Build checks:

```bash
dotnet build -f net10.0-windows10.0.19041.0
```

Platform-specific checks can use the commands from `AGENTS.md` when building on the
target platform.

## Suggested Implementation Order

1. Add `ExerciseTrackingMode` and model properties.
2. Add idempotent schema upgrade logic.
3. Update `AppDbContext` configuration.
4. Update `ExerciseDialog.razor`.
5. Update `SetTracker.razor` for reps-only mode.
6. Add `TimedExerciseTracker.razor`.
7. Update `ActiveSession.razor` to choose the correct tracker.
8. Add `SaveTimedExerciseAsync`.
9. Update history rendering.
10. Update dashboard/reporting filters.
11. Add localization keys.
12. Run build and manual smoke tests.
