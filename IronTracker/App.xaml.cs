using IronTracker.Services;

namespace IronTracker;

public partial class App : Application
{
	private SettingsService? _settingsService;
	private System.Timers.Timer? _saveWindowSizeTimer;

	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new MainPage()) { Title = "IronTracker" };

		// Get SettingsService from the service provider
		_settingsService = Handler?.MauiContext?.Services.GetService<SettingsService>();

		if (_settingsService != null)
		{
			// Restore window size and position
			window.Width = _settingsService.WindowWidth;
			window.Height = _settingsService.WindowHeight;

			// Only restore position if it was previously saved (not -1)
			if (_settingsService.WindowX >= 0 && _settingsService.WindowY >= 0)
			{
				window.X = _settingsService.WindowX;
				window.Y = _settingsService.WindowY;
			}

			// Initialize debounce timer for size changes (500ms delay)
			_saveWindowSizeTimer = new System.Timers.Timer(500);
			_saveWindowSizeTimer.AutoReset = false;
			_saveWindowSizeTimer.Elapsed += (sender, e) =>
			{
				if (_settingsService != null && window.Width > 0 && window.Height > 0)
				{
					_settingsService.WindowWidth = window.Width;
					_settingsService.WindowHeight = window.Height;
				}
			};

			// Save window size with debouncing when it changes
			window.SizeChanged += (sender, e) =>
			{
				// Reset timer on each size change to debounce rapid resizing
				_saveWindowSizeTimer?.Stop();
				_saveWindowSizeTimer?.Start();
			};

			// Track window position changes
			window.Destroying += (sender, e) =>
			{
				// Ensure any pending size save completes
				if (_saveWindowSizeTimer?.Enabled == true)
				{
					_saveWindowSizeTimer.Stop();
					if (_settingsService != null && window.Width > 0 && window.Height > 0)
					{
						_settingsService.WindowWidth = window.Width;
						_settingsService.WindowHeight = window.Height;
					}
				}

				// Save position when window is closing
				if (_settingsService != null && window.X >= 0 && window.Y >= 0)
				{
					_settingsService.WindowX = window.X;
					_settingsService.WindowY = window.Y;
				}

				// Cleanup
				_saveWindowSizeTimer?.Dispose();
			};
		}

		return window;
	}
}
