using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using WinDash2.Core;
using WinDash2.Views;

namespace WinDash2;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        try
        {
            WidgetManager s = new WidgetManager(new Services.WidgetFileSystemService("C:\\Users\\davei\\AppData\\Roaming\\windash2\\widgets"));
            s.InitializeAsync().GetAwaiter().GetResult();
            //_window = new WidgetWindow("https://open.spotify.com/");
            //_window.Activate();
            //new ManagerWindow().Activate();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Unhandled exception during launch: " + ex);
        }
    }
}
