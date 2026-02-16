namespace IronTracker;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	/// <summary>
	/// Reloads the BlazorWebView to apply culture changes.
	/// This must be called after changing the culture to force a complete reload.
	/// </summary>
	public void ReloadWebView()
	{
#if ANDROID
		var androidWebView = blazorWebView.Handler?.PlatformView as Android.Webkit.WebView;
		androidWebView?.LoadUrl("file:///android_asset/wwwroot/index.html");
#elif IOS || MACCATALYST
		var wkWebView = blazorWebView.Handler?.PlatformView as WebKit.WKWebView;
		wkWebView?.Reload();
#elif WINDOWS
		var webView2 = blazorWebView.Handler?.PlatformView as Microsoft.UI.Xaml.Controls.WebView2;
		if (webView2?.CoreWebView2 != null)
		{
			webView2.CoreWebView2.Reload();
		}
#endif
	}
}
