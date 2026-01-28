# IronTracker - Agent Guidelines

This document provides coding agents with essential information about the IronTracker codebase, including build commands, code style, and architectural patterns.

## Project Overview

**Type**: Cross-platform workout tracking app  
**Framework**: .NET 10 MAUI Blazor Hybrid  
**UI Library**: MudBlazor 8.0.0  
**Database**: Entity Framework Core 9.0.1 with SQLite  
**Platforms**: macOS (Catalyst), iOS, Android, Windows  

## Build & Run Commands

All commands should be run from the `IronTracker/` directory (not the parent directory).

### Build
```bash
# macOS (Catalyst) - Primary development platform
dotnet build -f net10.0-maccatalyst

# iOS
dotnet build -f net10.0-ios

# Android
dotnet build -f net10.0-android

# Windows (only available on Windows OS)
dotnet build -f net10.0-windows10.0.19041.0
```

### Run
```bash
# macOS (Catalyst)
dotnet run -f net10.0-maccatalyst

# iOS (requires device/simulator)
dotnet run -f net10.0-ios

# Android (requires emulator/device)
dotnet run -f net10.0-android
```

### Clean Build
```bash
dotnet clean
dotnet build -f net10.0-maccatalyst
```

## Testing

**Status**: No test project currently exists in the solution.

**To add tests**:
1. Create a new xUnit or NUnit test project: `IronTracker.Tests`
2. Add reference to main project
3. Test naming convention: `MethodName_Scenario_ExpectedResult`
4. Run tests: `dotnet test`

**Unit test individual components**:
```bash
# When test project exists
dotnet test --filter "FullyQualifiedName~IronTracker.Tests.Services"
dotnet test --filter "FullyQualifiedName~WorkoutServiceTests.SaveSetAsync_ValidData_ReturnsSetLog"
```

## Code Style Guidelines

### General Formatting
- **Indentation**: 4 spaces (no tabs)
- **Line endings**: LF (Unix-style)
- **File encoding**: UTF-8
- **Max line length**: No strict limit, but prefer ~120 chars for readability
- **Braces**: K&R style (opening brace on same line for methods/classes)

### Naming Conventions
- **Classes/Interfaces**: PascalCase (`WorkoutService`, `IWorkoutRepository`)
- **Methods/Properties**: PascalCase (`GetRoutinesAsync`, `TargetConfig`)
- **Private fields**: camelCase with underscore prefix (`_repository`, `_sessionManager`)
- **Local variables**: camelCase (`sessionId`, `exerciseName`)
- **Constants**: PascalCase (`DefaultRestSeconds`)
- **Async methods**: Always suffix with `Async`

### C# Features
- **Nullable reference types**: ENABLED - Use `?` for nullable types, `= null!` for DI-injected properties
- **Implicit usings**: ENABLED in project settings
- **String handling**: Prefer string interpolation `$"{var}"` over concatenation
- **LINQ**: Use liberally for collections
- **Expression-bodied members**: Use for simple one-liners
  ```csharp
  public Task<List<Routine>> GetRoutinesAsync() => _repository.GetRoutinesAsync();
  ```
- **Pattern matching**: Use modern C# patterns (switch expressions, is patterns)
- **File-scoped namespaces**: Optional but preferred for new files

### Type Conventions
- Use `var` when type is obvious from right-hand side
- Explicit types for primitive types and when clarity is needed
- Always use `async Task<T>` for async methods (not `async void` except event handlers)
- Use `List<T>` for return types, `IEnumerable<T>` for parameters when possible
- Prefer `string.Empty` over `""`
- Use `decimal` for currency/weights, `int` for counts, `DateTime` for timestamps

### Imports
Standard import order (handled by implicit usings):
1. System namespaces (auto-included)
2. Third-party packages (Microsoft.*, MudBlazor.*)
3. Project namespaces (IronTracker.*)

Example:
```csharp
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using IronTracker.Models;
using IronTracker.Services.Interfaces;
```

## Architecture & Patterns

### Project Structure
```
IronTracker/
├── Components/
│   ├── Layout/          # MainLayout, NavMenu
│   ├── Pages/           # Blazor pages (Home, ActiveSession, Routines, History)
│   └── *.razor          # Reusable components and dialogs
├── Data/
│   ├── AppDbContext.cs  # EF Core DbContext
│   └── DbSeeder.cs      # Database initialization
├── Models/              # Domain models (entities)
├── Services/
│   ├── Interfaces/      # Service contracts
│   └── *.cs            # Service implementations
├── Platforms/           # Platform-specific code
└── wwwroot/            # Static web assets
```

### Dependency Injection
All services use constructor injection. Register in `MauiProgram.cs`:
```csharp
builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
builder.Services.AddScoped<WorkoutService>();
```

Inject in components:
```csharp
[Inject]
private WorkoutService WorkoutService { get; set; } = null!;
```

### Service Layer Pattern
- **WorkoutService**: High-level business logic, orchestrates repository + session manager
- **WorkoutRepository**: Data access (CRUD operations)
- **SessionManager**: Active session state management
- **Interfaces**: Define contracts in `Services/Interfaces/`

### Database (EF Core)
- **DbContext**: `AppDbContext` in `Data/` folder
- **Migrations**: Use `dotnet ef migrations add MigrationName` (from IronTracker/ directory)
- **Connection string**: SQLite at `FileSystem.AppDataDirectory/irontracker.db`
- **Conventions**:
  - Navigation properties always initialized: `= null!`
  - Use `IsRequired()`, `HasMaxLength()` in `OnModelCreating`
  - Add indexes for frequently queried fields
  - Cascade deletes for parent-child relationships

### Blazor Components
- **File naming**: PascalCase with `.razor` extension
- **Code-behind**: Use `@code` blocks in .razor files (no separate .cs files)
- **Parameters**: Mark with `[Parameter]` attribute
- **Cascading parameters**: Use `[CascadingParameter]` for MudBlazor dialogs
- **Event handling**: Use `OnClick`, `@bind-Value`, `@bind-Value:after` for events
- **Lifecycle**: Prefer `OnInitializedAsync` for async initialization

### Error Handling
- **Validation**: Use MudBlazor form validation and `IsValid()` helper methods
- **Async exceptions**: Let bubble up to component level, show MudBlazor snackbar
- **Null safety**: Leverage nullable reference types, use null-conditional operators
- **Database errors**: Wrap in try-catch at service layer when appropriate

## Documentation Standards

### XML Documentation
ALL public APIs (classes, methods, properties) must have XML doc comments:

```csharp
/// <summary>
/// Brief description of what this does.
/// </summary>
/// <param name="paramName">What this parameter represents</param>
/// <returns>What is returned</returns>
public async Task<WorkoutSession> StartSessionAsync(int routineDayId)
```

### Code Comments
- Use `//` for inline comments explaining "why", not "what"
- Razor comments: `@* Comment *@`
- Avoid obvious comments; code should be self-documenting

## Platform-Specific Notes

### File Paths
Always use `FileSystem.AppDataDirectory` for cross-platform compatibility:
```csharp
var dbPath = Path.Combine(FileSystem.AppDataDirectory, "irontracker.db");
```

### Multi-targeting
Project targets multiple frameworks. Use conditional compilation if needed:
```csharp
#if ANDROID
    // Android-specific code
#elif IOS
    // iOS-specific code
#endif
```

## Common Patterns

### Async/Await
Always use async/await for database and service calls:
```csharp
var routines = await WorkoutService.GetRoutinesAsync();
```

### String Parsing
Use `TryParse` for safe conversions:
```csharp
if (int.TryParse(input, out var result))
{
    // Use result
}
```

### LINQ Queries
Prefer method syntax for readability:
```csharp
var exercises = day.Exercises
    .Where(e => e.IsActive)
    .OrderBy(e => e.SortOrder)
    .ToList();
```

## Git Workflow

- Commit messages: Present tense ("Add feature" not "Added feature")
- Keep commits focused and atomic
- No secrets or connection strings in source control
