# IronTracker

A cross-platform workout tracking app built with .NET MAUI Blazor Hybrid and MudBlazor.

## Overview

IronTracker helps you track your gym workouts with a clean, modern interface. Built for macOS, iOS, Android, and Windows.

## Features

- **Routine Management**: Create custom workout routines with multiple training days
- **Exercise Templates**: Define exercises with target reps and weights per set
- **Smart Weight Tracking**: 
  - Auto-suggests weights with 5lbs increments
  - Remembers weights from previous sessions
  - Pre-fills weights from exercise templates
- **Active Session Tracking**: 
  - Track sets, reps, and weights in real-time
  - Built-in rest timer (60 seconds default)
  - Responsive design (desktop 2-column, mobile accordion)
- **Workout History**: View past sessions with date range filtering
- **Dashboard**: Track workout streaks and volume over time

## Tech Stack

- **.NET 10.0** with .NET MAUI
- **Blazor Hybrid** for cross-platform UI
- **MudBlazor 8.0.0** for Material Design components
- **Entity Framework Core 9.0.1** with SQLite for local data storage

## Getting Started

### Prerequisites

- .NET 10 SDK
- Xcode (for macOS/iOS development)
- Visual Studio 2022 or VS Code with C# extensions

### Build & Run

```bash
cd IronTracker

# For macOS (Catalyst)
dotnet build -f net10.0-maccatalyst
dotnet run -f net10.0-maccatalyst

# For Windows
dotnet build -f net10.0-windows10.0.19041.0
dotnet run -f net10.0-windows10.0.19041.0
```

## Project Structure

```
IronTracker/
├── Components/
│   ├── Layout/          # MainLayout, NavMenu
│   ├── Pages/           # Home, ActiveSession, Routines, History
│   ├── RestTimer.razor
│   ├── SetTracker.razor
│   └── Dialogs/         # RoutineDialog, DayDialog, ExerciseDialog
├── Data/
│   ├── AppDbContext.cs  # EF Core context
│   └── DbSeeder.cs      # Initial data seeding
├── Models/              # Data models (Routine, ExerciseTemplate, SetLog, etc.)
├── Services/            # WorkoutService, WorkoutRepository, SessionManager
└── wwwroot/             # Static assets
```

## Database

Uses SQLite with Entity Framework Core for local storage. Database is created automatically on first run at:
- **macOS**: `~/Library/Containers/com.companyname.irontracker/Data/Library/irontracker.db`
- **Windows**: Local app data folder

## Usage

### Creating a Routine

1. Navigate to **Routines** page
2. Click **New Routine**
3. Add training days (e.g., "Chest Day", "Back Day")
4. Add exercises to each day with:
   - Exercise name
   - Target reps per set (e.g., "15-12-10-8")
   - Target weights per set (e.g., "20-25-30-35") - optional

### Starting a Workout

1. Go to **Routines** or **Active Session**
2. Select a training day to start
3. Track each set with reps and weight
4. Use the built-in rest timer between sets
5. Complete all exercises and end session

### Smart Weight Features

- **Auto-suggest**: Type first weight (e.g., "20"), get suggestion for remaining sets with 5lbs increments
- **Weight memory**: Next session auto-fills weights from your last workout
- **Template weights**: Define default weights in exercise templates

## License

MIT License - see [LICENSE](LICENSE) file for details

## Author

Roberto Saban

## Contributing

This is a personal project, but feel free to fork and customize for your own use.
