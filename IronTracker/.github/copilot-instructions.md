# IronTracker - AI Development Guide

## Project Overview

**IronTracker** is a cross-platform workout tracking app built with **.NET MAUI** (net10.0) + **Blazor Hybrid** (MudBlazor UI). It runs on iOS, Android, macOS Catalyst, and Windows. The app allows users to create workout routines, track sets/reps during active sessions, and view history/statistics.

## Architecture

### Hybrid App Structure

- **Entry Point**: [MauiProgram.cs](../MauiProgram.cs) configures DI, registers MudBlazor, sets up SQLite with EF Core
- **XAML Shell**: [MainPage.xaml](../MainPage.xaml) hosts a `BlazorWebView` that loads Blazor components
- **Blazor Router**: [Components/Routes.razor](../Components/Routes.razor) handles client-side routing
- **Root Layout**: [Components/Layout/MainLayout.razor](../Components/Layout/MainLayout.razor) provides MudBlazor shell (app bar, drawer, theme toggle)

### Data Layer (Entity Framework Core + SQLite)

- **DbContext**: [Data/AppDbContext.cs](../Data/AppDbContext.cs) - configurations for all entities with cascade delete rules
- **Models**: `Routine` → `RoutineDay` → `ExerciseTemplate` hierarchy; `WorkoutSession` → `SetLog` for tracking actual workouts
- **Database Path**: `FileSystem.AppDataDirectory/irontracker.db` (platform-specific location)
- **Seeding**: [Data/DbSeeder.cs](../Data/DbSeeder.cs) auto-seeds sample "Rutina Roberto Saban" (3-day Push/Pull/Legs split) on first run
- **Pattern**: Use `IDbContextFactory<AppDbContext>` everywhere (required for Blazor threading safety). Never inject `AppDbContext` directly.

### Service Layer

- **WorkoutService**: High-level orchestrator combining repository + session management
- **IWorkoutRepository**: CRUD operations for routines, days, exercises, history queries
- **SessionManager**: Manages single active session at a time (auto-ends previous sessions when starting new)
- **Pattern**: Services are `Scoped` lifetime. All async operations use `await using var context = await _contextFactory.CreateDbContextAsync()`

### Active Session Workflow

1. User selects a routine day → [Services/SessionManager.cs](../Services/SessionManager.cs) creates `WorkoutSession` with `StartTime`, sets `EndTime=null`
2. [Components/Pages/ActiveSession.razor](../Components/Pages/ActiveSession.razor) renders exercise list + set tracker
3. [Components/SetTracker.razor](../Components/SetTracker.razor) component tracks sets for one exercise (input reps/weight, mark complete)
4. Each completed set creates a `SetLog` record linked to the active session
5. User clicks "End Session" → sets `EndTime`, calculates `TotalVolume`

### Key Conventions

- **Navigation Properties**: Always eagerly load with `.Include()` (e.g., `RoutineDay.Exercises`, `WorkoutSession.SetLogs`)
- **Ordering**: Use `SortOrder` property for exercises/days (not relying on ID)
- **UTC Storage**: All `DateTime` fields stored as UTC; convert to local time in UI (`ToLocalTime()`)
- **Computed Properties**: Models have calculated properties (e.g., `WorkoutSession.TotalVolume`, `IsActive`, `Duration`) - never stored in DB
- **Nullable Reference Types**: Enabled project-wide; use `= null!` for EF Core navigation properties initialized by framework

## Development Workflows

### Running the App

```bash
# Build and run on Mac Catalyst (most common for development)
dotnet build -f net10.0-maccatalyst
dotnet run -f net10.0-maccatalyst

# Or use VS Code launch configurations
# iOS Simulator / Android Emulator require platform-specific setup
```

### Database Migrations

```bash
# Add migration (must specify -f flag for single target framework)
dotnet ef migrations add MigrationName -f net10.0-maccatalyst

# Apply migrations (happens auto via EnsureCreatedAsync in MauiProgram)
# No manual migration needed - using Code First with EnsureCreated pattern
```

### Debugging

- Use `#if DEBUG` blocks - project has `AddBlazorWebViewDeveloperTools()` enabled for hot reload
- SQLite database inspector: Use VS Code SQLite extension or Datagrip to inspect `irontracker.db` in app data directory
- Navigation: All pages use `@page` directive; navigate via `NavigationManager.NavigateTo("/path")`

## UI Patterns (MudBlazor)

### Dialogs

- Use `IDialogService` injected in pages
- Example: [Components/ExerciseDialog.razor](../Components/ExerciseDialog.razor), [Components/RoutineDialog.razor](../Components/RoutineDialog.razor)
- Pattern: `DialogParameters` for input, `DialogReference.Close(result)` to return data

### Responsive Design

- Use `<MudHidden Breakpoint="...">` to show/hide content (e.g., [ActiveSession.razor](../Components/Pages/ActiveSession.razor) has desktop 2-column vs mobile accordion layout)
- MudBlazor breakpoints: `Xs`, `Sm`, `Md`, `Lg`, `Xl`

### Dark Mode

- Controlled by `MainLayout.razor` with `MudThemeProvider` + `@bind-IsDarkMode`
- Checks system preference on first render: `await _themeProvider.GetSystemPreference()`

## Important Gotchas

1. **DbContext Threading**: Always use `IDbContextFactory` - Blazor can run on different threads
2. **Active Session Singleton**: Only ONE active session allowed system-wide (enforced by `SessionManager.StartSessionAsync`)
3. **Cascade Deletes**: Deleting `Routine` cascades to `RoutineDay` and `ExerciseTemplate`; deleting `WorkoutSession` cascades to `SetLog`
4. **Platform Paths**: Use `FileSystem.AppDataDirectory` for cross-platform storage, never hardcode paths
5. **MudBlazor StateHasChanged**: In event handlers, call `StateHasChanged()` if modifying data from child components

## Adding New Features

### New Model + Migration

1. Create model in `Models/` with navigation properties
2. Add `DbSet<T>` to [AppDbContext.cs](../Data/AppDbContext.cs)
3. Configure relationships in `OnModelCreating`
4. Generate migration: `dotnet ef migrations add AddFeature -f net10.0-maccatalyst`

### New Page

1. Create `.razor` file in `Components/Pages/`
2. Add `@page "/route"` directive
3. Add navigation link in [NavMenu.razor](../Components/Layout/NavMenu.razor)
4. Inject services via `@inject ServiceName VariableName`

### New Service

1. Create interface in `Services/Interfaces/`
2. Implement in `Services/`
3. Register in [MauiProgram.cs](../MauiProgram.cs): `builder.Services.AddScoped<IService, ServiceImpl>()`
