# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Workspace Structure

**IronTracker** is a cross-platform workout tracking app built with **.NET 10 MAUI + Blazor Hybrid**. The workspace has this layout:

```
/
├── IronTracker/              # Main application code
│   ├── Components/           # Blazor components (.razor files)
│   ├── Data/                 # AppDbContext, DbSeeder
│   ├── Models/               # Domain entities
│   ├── Services/             # Business logic layer
│   ├── Platforms/            # Platform-specific code
│   ├── Resources/            # Icons, fonts, localization
│   ├── wwwroot/              # Static assets
│   ├── CLAUDE.md             # Detailed architecture guide
│   └── IronTracker.csproj    # Project file
├── AGENTS.md                 # Coding guidelines and patterns
├── README.md                 # User-facing documentation
├── CHANGELOG.md              # Version history
├── .editorconfig             # Code style rules (enforced)
└── .vscode/                  # VSCode tasks and launch configs
```

**Important**: All development happens in the `IronTracker/` subdirectory. Run build commands from there.

## Quick Start

### Build for macOS (Primary Platform)
```bash
cd IronTracker
dotnet build -f net10.0-maccatalyst
```

### Run on macOS
```bash
cd IronTracker
dotnet run -f net10.0-maccatalyst
```

### Build for Other Platforms
```bash
# Windows
dotnet build -f net10.0-windows10.0.19041.0

# iOS
dotnet build -f net10.0-ios
```

### Database Migrations
```bash
cd IronTracker

# Add migration
dotnet ef migrations add MigrationName -f net10.0-maccatalyst

# Update database
dotnet ef database update -f net10.0-maccatalyst
```

**Critical**: The `-f net10.0-maccatalyst` flag is required for EF Core commands because the project targets multiple frameworks.

### Clean & Rebuild
```bash
cd IronTracker
dotnet clean && dotnet build -f net10.0-maccatalyst
```

## Technology Stack

| Component | Version | Purpose |
|-----------|---------|---------|
| **.NET** | 10.0 | Runtime framework |
| **.NET MAUI** | Latest | Cross-platform app framework |
| **Blazor** | Hybrid | Web UI in native app |
| **MudBlazor** | 8.0.0 | Material Design components |
| **Entity Framework Core** | 9.0.1 | ORM for SQLite |
| **SQLite** | Latest | Local database storage |

## Project Structure Details

### Components (`IronTracker/Components/`)
Blazor `.razor` files (UI and logic combined):
- `Layout/` - MainLayout, NavMenu
- `Pages/` - Home, ActiveSession, Routines, History
- `Dialogs/` - RoutineDialog, DayDialog, ExerciseDialog
- `RestTimer.razor`, `SetTracker.razor` - Reusable components

### Data Layer (`IronTracker/Data/`)
- `AppDbContext.cs` - EF Core DbContext with all entities
- `DbSeeder.cs` - Populates sample data on first run

### Models (`IronTracker/Models/`)
Domain entities:
- `Routine` → `RoutineDay` → `ExerciseTemplate` (defining structure)
- `WorkoutSession` → `SetLog` (tracking actual workouts)

### Services (`IronTracker/Services/`)
Business logic:
- `WorkoutService` - High-level orchestrator
- `WorkoutRepository` - CRUD operations
- `SessionManager` - Active session state management
- Interfaces in `Services/Interfaces/`

## Code Style & Formatting

Code style is enforced by `.editorconfig`. Key rules:
- **Indentation**: 4 spaces (2 for JSON/XML)
- **Line endings**: LF (`\n`)
- **Max line length**: 120 characters
- **Naming**: PascalCase for classes/methods, `_camelCase` for private fields
- **Async methods**: Suffix with `Async`
- **Imports**: System first, then third-party, then project namespaces
- **Nullable**: Enabled project-wide (`<Nullable>enable</Nullable>`)
- **Braces**: Same-line (K&R style, enforced by editorconfig)

See `AGENTS.md` for detailed code style guidelines and patterns.

## Important Conventions

### Database & Threading
- **Always use `IDbContextFactory<AppDbContext>`** - Never inject `AppDbContext` directly (Blazor runs on different threads)
- Usage: `await using var context = await _contextFactory.CreateDbContextAsync()`

### Active Sessions
- Only ONE active session allowed system-wide (enforced by `SessionManager`)
- When user starts a new workout, previous session auto-ends

### Entity Framework
- Navigation properties: Use `= null!` initialization (EF Core sets them)
- All `DateTime` fields stored as UTC; convert to local time in UI with `.ToLocalTime()`
- Cascade delete rules configured in `AppDbContext.OnModelCreating()`

### Blazor Components
- Use `@code` blocks (no separate `.cs` files)
- Inject services with `@inject` directive or `[Inject]` attribute
- Call `StateHasChanged()` when modifying data from child components
- Use `MudDialog` for dialogs with `DialogParameters` for input/output

### Database File Location
- macOS Catalyst: `~/Library/Containers/dev.rsaban.irontracker/Data/Library/irontracker.db`
- Windows: `%LOCALAPPDATA%\...`
- Always use `FileSystem.AppDataDirectory` in code (cross-platform)

## Documentation to Reference

For detailed information about specific areas:
- **Architecture & Data Layer**: See `IronTracker/CLAUDE.md` - explains DbContext patterns, active session workflow, UI architecture
- **Coding Guidelines**: See `AGENTS.md` - patterns, naming conventions, error handling, platform-specific code
- **User Features**: See `README.md` - how the app works, feature list
- **Version History**: See `CHANGELOG.md` - recent changes and additions

## Common Tasks

### Adding a New Database Model
1. Create model class in `IronTracker/Models/`
2. Add `DbSet<YourModel>` to `IronTracker/Data/AppDbContext.cs`
3. Configure in `AppDbContext.OnModelCreating()`
4. Add EF migration from `IronTracker/` directory:
   ```bash
   dotnet ef migrations add AddYourModel -f net10.0-maccatalyst
   ```

### Adding a New Page
1. Create `.razor` file in `IronTracker/Components/Pages/`
2. Add `@page "/route"` directive at top
3. Add link in `IronTracker/Components/Layout/NavMenu.razor`
4. Inject services with `@inject ServiceType ServiceName`

### Running a Single Test
No unit tests exist in this project. Manual testing is performed on actual app platforms.

## Debugging Tips

### Database Issues
- Database file location varies by platform - check `FileSystem.AppDataDirectory` at runtime
- Use `dotnet ef database update -f net10.0-maccatalyst` to apply migrations
- `DbSeeder.cs` runs on first launch with sample data

### Blazor Component Issues
- If state doesn't update after service call, call `StateHasChanged()` in the component
- Check MudBlazor dialog return types match expected `DialogResult<T>`

### Performance
- The app uses `IDbContextFactory` to avoid blocking UI thread
- Always use `await` for async operations
- Navigation properties are eagerly loaded with `.Include()` in services

## Build Targets

This project builds for multiple platforms:
- **macOS Catalyst** (`net10.0-maccatalyst`) - Primary development target
- **Windows** (`net10.0-windows10.0.19041.0`) - Windows desktop
- **iOS** (`net10.0-ios`) - Apple mobile devices

Specify target framework with `-f` flag for all build/run commands.

## Git Conventions

- Commit messages in present tense ("Add feature" not "Added feature")
- Keep commits atomic (one logical change per commit)
- No secrets in source control
