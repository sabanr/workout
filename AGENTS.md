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
```

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
- Migrations: `dotnet ef migrations add Name -f net10.0-maccatalyst`
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

### Error Handling
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
