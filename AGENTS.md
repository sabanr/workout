# IronTracker - Agent Guidelines

Coding guidelines for the IronTracker workout tracking app.

## Project Overview

**Type**: Cross-platform workout tracking app  
**Framework**: .NET 10 MAUI Blazor Hybrid  
**UI Library**: MudBlazor 8.0.0  
**Database**: Entity Framework Core 9.0.1 with SQLite  
**Platforms**: macOS (Catalyst), iOS, Android, Windows

## Build Commands

Run from `IronTracker/` directory:
```bash
# Build
dotnet build -f net10.0-maccatalyst  # macOS (primary)
dotnet build -f net10.0-ios          # iOS
dotnet build -f net10.0-android      # Android
dotnet build -f net10.0-windows10.0.19041.0  # Windows

# Run
dotnet run -f net10.0-maccatalyst

# Clean
dotnet clean && dotnet build -f net10.0-maccatalyst

# EF Core Migrations
dotnet ef migrations add MigrationName -f net10.0-maccatalyst
dotnet ef database update -f net10.0-maccatalyst
```

## Code Style

### Formatting (enforced by .editorconfig)
- **Indent**: 4 spaces (2 for JSON/XML/CSProj)
- **Line endings**: LF (`\n`)
- **Max line length**: 120 characters
- **Charset**: UTF-8
- **Braces**: Same line (K&R style)

### Imports
```csharp
// 1. System namespaces first
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// 2. Third-party packages
using MudBlazor.Services;
using Plugin.Maui.Audio;

// 3. Project namespaces
using IronTracker.Data;
using IronTracker.Services;
```
- Sort system directives first
- Separate import directive groups with blank line
- Use file-scoped namespaces

### Naming Conventions
- **Classes/Interfaces/Methods/Properties**: PascalCase
- **Private fields**: `_camelCase` with underscore prefix
- **Constants**: PascalCase
- **Async methods**: Suffix with `Async`
- **Interfaces**: Prefix with `I`

### Types
- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Use `var` when type is apparent
- Navigation properties: `= null!` for EF Core
- Prefer `Task<T>` over `void` for async

## Architecture

### Project Structure
```
IronTracker/
├── Components/     # Blazor components
│   ├── Layout/     # MainLayout, NavMenu
│   ├── Pages/      # Blazor pages
│   └── *.razor     # Reusable components
├── Data/           # AppDbContext, DbSeeder
├── Models/         # Domain entities
├── Services/       # Business logic
│   ├── Interfaces/
│   └── *.cs
└── wwwroot/        # Static assets
```

### Dependency Injection
Register in `MauiProgram.cs`:
```csharp
builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
builder.Services.AddScoped<WorkoutService>();
```

Inject in components:
```csharp
[Inject]
private WorkoutService WorkoutService { get; set; } = null!;
```

### Service Pattern
- **WorkoutService**: High-level orchestrator
- **WorkoutRepository**: Data access (CRUD)
- **SessionManager**: Active session state
- Interfaces in `Services/Interfaces/`

### Database (EF Core)
- Use `IDbContextFactory<AppDbContext>` (never inject `AppDbContext` directly)
- SQLite at `FileSystem.AppDataDirectory/irontracker.db`
- Navigation properties: always `= null!`
- Use `IsRequired()`, `HasMaxLength()` in `OnModelCreating`
- Cascade deletes for parent-child

### Blazor Components
- PascalCase `.razor` files
- Use `@code` blocks (no separate .cs files)
- `[Parameter]` for parameters
- `[CascadingParameter]` for MudBlazor dialogs
- `OnClick`, `@bind-Value`, `@bind-Value:after` for events
- Prefer `OnInitializedAsync` for async init

## Error Handling
- MudBlazor form validation with `IsValid()`
- Async exceptions bubble to component level
- Use nullable reference types
- Database errors: try-catch at service layer

## Documentation

### XML Docs (required for all public APIs)
```csharp
/// <summary>Brief description.</summary>
/// <param name="paramName">Description</param>
/// <returns>Description</returns>
public async Task<WorkoutSession> StartSessionAsync(int routineDayId)
```

### Code Comments
- Use `//` for "why", not "what"
- Razor: `@* comment *@`
- Avoid obvious comments

## Platform Notes

### Database File Location

| Platform | Path |
|----------|------|
| **macOS Catalyst** | `~/Library/Containers/dev.rsaban.irontracker/Data/Library/irontracker.db` |
| **iOS** | `(app sandbox)/Library/irontracker.db` |
| **Android** | `/data/data/dev.rsaban.irontracker/files/irontracker.db` |
| **Windows** | `%LOCALAPPDATA%\Packages\dev.rsaban.irontracker\LocalState\irontracker.db` |

### File Paths
Always use `FileSystem.AppDataDirectory`:
```csharp
var dbPath = Path.Combine(FileSystem.AppDataDirectory, "irontracker.db");
```

### Conditional Compilation
```csharp
#if ANDROID
    // Android code
#elif IOS
    // iOS code
#endif
```

## Patterns

### DbContext Usage
```csharp
await using var context = await _contextFactory.CreateDbContextAsync();
var routines = await context.Routines.ToListAsync();
```

### Safe Parsing
```csharp
if (int.TryParse(input, out var result)) { /* use result */ }
```

### LINQ
```csharp
var exercises = day.Exercises
    .Where(e => e.IsActive)
    .OrderBy(e => e.SortOrder)
    .ToList();
```

## Gotchas

1. **DbContext Threading**: Always use `IDbContextFactory` - Blazor uses multiple threads
2. **Active Session**: Only ONE allowed system-wide (enforced by `SessionManager`)
3. **Cascade Deletes**: Deleting `Routine` → deletes `RoutineDay` → `ExerciseTemplate`
4. **UTC Storage**: All `DateTime` stored as UTC; use `ToLocalTime()` in UI
5. **StateHasChanged**: Call in event handlers when modifying data from child components

## Git

- Commit messages: Present tense ("Add feature" not "Added feature")
- Keep commits atomic
- No secrets in source control
