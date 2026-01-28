using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using IronTracker.Data;
using IronTracker.Services;
using IronTracker.Services.Interfaces;

namespace IronTracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // Add MudBlazor services
        builder.Services.AddMudServices();

        // Configure SQLite database
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "irontracker.db");
        builder.Services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Register services with DI
        builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
        builder.Services.AddScoped<ISessionManager, SessionManager>();
        builder.Services.AddScoped<WorkoutService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Initialize database and seed data
        InitializeDatabaseAsync(app.Services).GetAwaiter().GetResult();

        return app;
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        
        // Ensure database exists and seed data
        await DbSeeder.SeedAsync(contextFactory);
    }
}
