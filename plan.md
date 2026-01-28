# Plan de Implementación: IronTracker

## Resumen del Proyecto

| Aspecto | Detalle |
|---------|---------|
| **Framework** | .NET 9 MAUI Blazor Hybrid |
| **UI Library** | MudBlazor |
| **Database** | SQLite con EF Core |
| **Targets** | Windows, macOS, iOS, Android |

---

## 1. Estructura de Archivos

```
IronTracker/
├── IronTracker.sln
├── IronTracker/
│   ├── IronTracker.csproj
│   ├── MauiProgram.cs
│   ├── App.xaml / App.xaml.cs
│   ├── MainPage.xaml
│   │
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── DbSeeder.cs
│   │
│   ├── Models/
│   │   ├── Routine.cs
│   │   ├── RoutineDay.cs
│   │   ├── ExerciseTemplate.cs
│   │   ├── WorkoutSession.cs
│   │   └── SetLog.cs
│   │
│   ├── Services/
│   │   ├── Interfaces/
│   │   │   ├── IWorkoutRepository.cs
│   │   │   └── ISessionManager.cs
│   │   ├── WorkoutRepository.cs
│   │   ├── SessionManager.cs
│   │   └── WorkoutService.cs
│   │
│   ├── Components/
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor
│   │   │   └── NavMenu.razor
│   │   ├── SetTracker.razor
│   │   └── RestTimer.razor
│   │
│   ├── Pages/
│   │   ├── Home.razor (Dashboard)
│   │   ├── ActiveSession.razor
│   │   ├── Routines.razor
│   │   └── History.razor
│   │
│   └── wwwroot/
│       └── css/
│           └── app.css
```

---

## 2. Modelo de Datos (EF Core)

### Diagrama de Entidades

```
┌─────────────┐       ┌──────────────┐       ┌───────────────────┐
│   Routine   │ 1───* │  RoutineDay  │ 1───* │ ExerciseTemplate  │
├─────────────┤       ├──────────────┤       ├───────────────────┤
│ Id (PK)     │       │ Id (PK)      │       │ Id (PK)           │
│ Name        │       │ RoutineId(FK)│       │ RoutineDayId (FK) │
│ Description │       │ Name         │       │ Name              │
│ CreatedAt   │       │ SortOrder    │       │ TargetConfig      │
└─────────────┘       └──────────────┘       │ SortOrder         │
                                             └───────────────────┘

┌─────────────────┐       ┌─────────────────────────────────────┐
│ WorkoutSession  │ 1───* │             SetLog                  │
├─────────────────┤       ├─────────────────────────────────────┤
│ Id (PK)         │       │ Id (PK)                             │
│ RoutineDayId(FK)│       │ WorkoutSessionId (FK)               │
│ StartTime       │       │ ExerciseName (string, denormalized) │
│ EndTime?        │       │ SetNumber                           │
│ Notes?          │       │ RepsPerformed                       │
└─────────────────┘       │ WeightUsed (decimal)                │
                          │ CompletedAt                         │
                          └─────────────────────────────────────┘
```

> **Nota sobre `SetLog.ExerciseName`**: Se mantiene como string denormalizado para facilitar reportes históricos incluso si el ejercicio se renombra o elimina.

---

## 3. Seed Data (desde Excel)

Basándome en `rutina-gimnasio-v2.xlsx`:

**Rutina**: "Rutina Roberto Saban"

| Día | Nombre | Ejercicios |
|-----|--------|------------|
| 1 | Pecho/Hombro/Tríceps | Press Plano (15-15-12-10), Press Inclinado (15-15-12-10), Aperturas (10-10-10-10), Press Arnold (12-12-10-10), Vuelo Lateral (12-12-12-12), Extensión Tríceps (12-12-12-12), Extensión Transnuca (12-12-12-12) |
| 2 | Espalda/Bíceps | Tirón al Pecho (15-15-12-10), Remo Bajo (15-15-12-10), Serrucho (10-10-10-10), Pull-over (10-10-10-10), Curl con Barra (15-15-12-12), Alternado Mancuerna (15-15-12-12) |
| 3 | Piernas | Prensa 45° (15-12-10-10), Hack (15-12-10-10), Extensión Cuádriceps (15-12-10-10), Curl Femoral (15-12-10-10), Gemelos (15-15-15-15) |

---

## 4. Servicios (Inyección de Dependencias)

### Interfaces

```csharp
// IWorkoutRepository - Acceso a datos
- GetRoutinesAsync()
- GetRoutineDayWithExercisesAsync(int dayId)
- GetSessionHistoryAsync(DateTime from, DateTime to)

// ISessionManager - Gestión de sesión activa
- StartSessionAsync(int routineDayId)
- GetActiveSessionAsync()
- EndSessionAsync()
- SaveSetAsync(SetLog set)
```

### Registro en DI (MauiProgram.cs)

```csharp
builder.Services.AddDbContextFactory<AppDbContext>();
builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
builder.Services.AddScoped<ISessionManager, SessionManager>();
builder.Services.AddScoped<WorkoutService>();
```

---

## 5. UI/UX - Páginas Principales

### A. Active Session (El Core)

**Layout Responsive:**

```
┌─────────────────────────────────────────────────────────────┐
│  DESKTOP (≥960px)                                           │
├─────────────────────────┬───────────────────────────────────┤
│  Lista de Ejercicios    │   Ejercicio Actual                │
│  ┌───────────────────┐  │   ┌─────────────────────────────┐ │
│  │ ✓ Press Plano     │  │   │  PRESS INCLINADO            │ │
│  │ → Press Inclinado │  │   │  Meta: 15-15-12-10          │ │
│  │   Aperturas       │  │   ├─────────────────────────────┤ │
│  │   Press Arnold    │  │   │ Serie │ Reps │ Peso │ Done  │ │
│  │   ...             │  │   │   1   │ [15] │[10.0]│  ☐   │ │
│  └───────────────────┘  │   │   2   │ [15] │[15.0]│  ☐   │ │
│                         │   │   3   │ [ ] │[ ]│  ☐   │ │
│                         │   │   4   │ [ ] │[ ]│  ☐   │ │
│                         │   └─────────────────────────────┘ │
│                         │   ┌─────────────────────────────┐ │
│                         │   │      REST TIMER             │ │
│                         │   │        01:00                │ │
│                         │   │   [SKIP]  [+30s]            │ │
│                         │   └─────────────────────────────┘ │
└─────────────────────────┴───────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│  MOBILE (<960px) - Accordion/Stepper                        │
├─────────────────────────────────────────────────────────────┤
│  ▼ Press Plano ✓                                            │
│  ▼ Press Inclinado (expandido)                              │
│    ┌─────────────────────────────────────────────────────┐  │
│    │ Serie 1: [15] reps × [10.0] kg  [✓]                 │  │
│    │ Serie 2: [15] reps × [15.0] kg  [✓]                 │  │
│    │ Serie 3: [ ] reps × [ ] kg  [ ]                 │  │
│    └─────────────────────────────────────────────────────┘  │
│  ▶ Aperturas                                                │
│  ▶ Press Arnold                                             │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  REST: 01:00   [SKIP]                               │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

**Componente SetTracker:**
- Parse de `TargetConfig` ("15-12-10-8") → genera 4 filas
- Inputs: `MudNumericField` para Reps y Weight
- Al marcar "Done" → guarda en DB + inicia Timer

**Componente RestTimer:**
- Timer de cuenta regresiva (default: 60 segundos)
- Se activa automáticamente al completar una serie
- Botón "Skip" para saltar el descanso
- Botón "+30s" para extender

### B. Dashboard (Home)

```
┌─────────────────────────────────────────────────────────────┐
│  🔥 Racha: 5 días consecutivos                              │
├─────────────────────────────────────────────────────────────┤
│  Volumen Semanal (kg)                                       │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  █████  ███████  ██████  ████████  ██████           │    │
│  │  Sem1   Sem2     Sem3    Sem4      Sem5             │    │
│  └─────────────────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────────────┤
│  [Iniciar Sesión ▶]                                         │
│  Última sesión: Hace 2 días - Día de Piernas                │
└─────────────────────────────────────────────────────────────┘
```

---

## 6. Detalles Técnicos Críticos

### Path de Base de Datos
```csharp
var dbPath = Path.Combine(FileSystem.AppDataDirectory, "irontracker.db");
```

### DbContextFactory (para Blazor Hybrid)
```csharp
// En el servicio, inyectar IDbContextFactory
public class WorkoutRepository : IWorkoutRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    
    public async Task<List<Routine>> GetRoutinesAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Routines.ToListAsync();
    }
}
```

### Cálculo de Volumen
```csharp
// Volumen = Σ(reps × peso)
var weeklyVolume = await context.SetLogs
    .Where(s => s.CompletedAt >= startOfWeek)
    .SumAsync(s => s.RepsPerformed * s.WeightUsed);
```

---

## 7. Paquetes NuGet Requeridos

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.*" />
<PackageReference Include="MudBlazor" Version="7.*" />
```

---

## 8. Orden de Implementación

| # | Tarea | Archivos |
|---|-------|----------|
| 1 | Crear proyecto MAUI Blazor Hybrid | `dotnet new maui-blazor` |
| 2 | Configurar MudBlazor | `MauiProgram.cs`, `_Imports.razor` |
| 3 | Crear Modelos/Entidades | `Models/*.cs` |
| 4 | Crear AppDbContext + Migración | `Data/AppDbContext.cs` |
| 5 | Crear Seed Data | `Data/DbSeeder.cs` |
| 6 | Crear Interfaces de Servicios | `Services/Interfaces/*.cs` |
| 7 | Implementar Servicios | `Services/*.cs` |
| 8 | Crear Layout y NavMenu | `Components/Layout/*.razor` |
| 9 | Crear componentes SetTracker y RestTimer | `Components/*.razor` |
| 10 | Crear página ActiveSession | `Pages/ActiveSession.razor` |
| 11 | Crear Dashboard | `Pages/Home.razor` |

---

## 9. Decisiones Tomadas

| Decisión | Valor |
|----------|-------|
| Timer default | 60 segundos |
| Cálculo volumen | Σ(reps × peso) |
| Seed data | Rutina de 3 días desde Excel |
| Layout responsive | 2 columnas (desktop) / Accordion (mobile) |
