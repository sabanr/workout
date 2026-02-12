# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**IronTracker** is a cross-platform workout tracking app built with **.NET MAUI** (net10.0) + **Blazor Hybrid** (MudBlazor UI). It runs on iOS, macOS Catalyst, and Windows. The app allows users to create workout routines, track sets/reps during active sessions, and view history/statistics.

## Build and Run Commands

```bash
# Build for Mac Catalyst (primary development platform)
dotnet build -f net10.0-maccatalyst

# Run on Mac Catalyst
dotnet run -f net10.0-maccatalyst

# Build for iOS
dotnet build -f net10.0-ios

# Build for Windows (Windows only)
dotnet build -f net10.0-windows10.0.19041.0

# Add EF Core migration (MUST specify target framework)
dotnet ef migrations add MigrationName -f net10.0-maccatalyst
```

## Architecture

### Hybrid App Structure

- **Entry Point**: `MauiProgram.cs` configures DI, registers MudBlazor, sets up SQLite with EF Core
- **XAML Shell**: `MainPage.xaml` hosts a `BlazorWebView` that loads Blazor components
- **Blazor Router**: `Components/Routes.razor` handles client-side routing
- **Root Layout**: `Components/Layout/MainLayout.razor` provides MudBlazor shell (app bar, drawer, theme toggle)

### Data Layer (Entity Framework Core + SQLite)

- **DbContext**: `Data/AppDbContext.cs` - configurations for all entities with cascade delete rules
- **Models**: `Routine` → `RoutineDay` → `ExerciseTemplate` hierarchy; `WorkoutSession` → `SetLog` for tracking actual workouts
- **Database Path**: `FileSystem.AppDataDirectory/irontracker.db` (platform-specific location)
- **Seeding**: `Data/DbSeeder.cs` auto-seeds sample "Rutina Roberto Saban" (3-day Push/Pull/Legs split) on first run
- **CRITICAL**: Always use `IDbContextFactory<AppDbContext>` everywhere (required for Blazor threading safety). Never inject `AppDbContext` directly.

### Service Layer

- **WorkoutService**: High-level orchestrator combining repository + session management
- **IWorkoutRepository**: CRUD operations for routines, days, exercises, history queries
- **SessionManager**: Manages single active session at a time (auto-ends previous sessions when starting new)
- **Pattern**: Services are `Scoped` lifetime. All async operations use `await using var context = await _contextFactory.CreateDbContextAsync()`

### Active Session Workflow

1. User selects a routine day → `Services/SessionManager.cs` creates `WorkoutSession` with `StartTime`, sets `EndTime=null`
2. `Components/Pages/ActiveSession.razor` renders exercise list + set tracker
3. `Components/SetTracker.razor` component tracks sets for one exercise (input reps/weight, mark complete)
4. Each completed set creates a `SetLog` record linked to the active session
5. User clicks "End Session" → sets `EndTime`, calculates `TotalVolume`

## Key Conventions

- **Navigation Properties**: Always eagerly load with `.Include()` (e.g., `RoutineDay.Exercises`, `WorkoutSession.SetLogs`)
- **Ordering**: Use `SortOrder` property for exercises/days (not relying on ID)
- **UTC Storage**: All `DateTime` fields stored as UTC; convert to local time in UI (`ToLocalTime()`)
- **Computed Properties**: Models have calculated properties (e.g., `WorkoutSession.TotalVolume`, `IsActive`, `Duration`) - never stored in DB
- **Nullable Reference Types**: Enabled project-wide; use `= null!` for EF Core navigation properties initialized by framework

## Important Gotchas

1. **DbContext Threading**: Always use `IDbContextFactory` - Blazor can run on different threads
2. **Active Session Singleton**: Only ONE active session allowed system-wide (enforced by `SessionManager.StartSessionAsync`)
3. **Cascade Deletes**: Deleting `Routine` cascades to `RoutineDay` and `ExerciseTemplate`; deleting `WorkoutSession` cascades to `SetLog`
4. **Platform Paths**: Use `FileSystem.AppDataDirectory` for cross-platform storage, never hardcode paths
5. **MudBlazor StateHasChanged**: In event handlers, call `StateHasChanged()` if modifying data from child components
6. **EF Core Migrations**: Must use `-f net10.0-maccatalyst` flag when adding migrations (project targets multiple frameworks)
7. **Database Creation**: Uses `EnsureCreatedAsync` pattern in `MauiProgram` - no manual migration application needed

## UI Patterns (MudBlazor)

### Dialogs

- Use `IDialogService` injected in pages
- Example: `Components/ExerciseDialog.razor`, `Components/RoutineDialog.razor`
- Pattern: `DialogParameters` for input, `DialogReference.Close(result)` to return data

### Responsive Design

- Use `<MudHidden Breakpoint="...">` to show/hide content (e.g., `ActiveSession.razor` has desktop 2-column vs mobile accordion layout)
- MudBlazor breakpoints: `Xs`, `Sm`, `Md`, `Lg`, `Xl`

### Dark Mode

- Controlled by `MainLayout.razor` with `MudThemeProvider` + `@bind-IsDarkMode`
- Checks system preference on first render: `await _themeProvider.GetSystemPreference()`

## Adding New Features

### New Model + Migration

1. Create model in `Models/` with navigation properties
2. Add `DbSet<T>` to `Data/AppDbContext.cs`
3. Configure relationships in `OnModelCreating`
4. Generate migration: `dotnet ef migrations add AddFeature -f net10.0-maccatalyst`

### New Page

1. Create `.razor` file in `Components/Pages/`
2. Add `@page "/route"` directive
3. Add navigation link in `Components/Layout/NavMenu.razor`
4. Inject services via `@inject ServiceName VariableName`

### New Service

1. Create interface in `Services/Interfaces/`
2. Implement in `Services/`
3. Register in `MauiProgram.cs`: `builder.Services.AddScoped<IService, ServiceImpl>()`

## Localization

- **Supported Languages**: English (en-US) and Spanish (es-AR) only
- **SatelliteResourceLanguages**: Configured in `.csproj` to limit satellite assemblies
- **Post-Publish Cleanup**: Custom MSBuild target removes unwanted language folders after publish
