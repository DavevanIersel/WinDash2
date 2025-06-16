using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.UI.Xaml;
using WinDash2.Core;
using WinDash2.Services;
using WinDash2.Views;

namespace WinDash2;

public partial class App : Application
{
    private Window? _window;
    private TrayManager? _trayManager;
    public static IHost AppHost { get; private set; }

    public App()
    {
        InitializeComponent();

        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register services
                services.AddSingleton<WidgetFileSystemService>(provider =>
                    new WidgetFileSystemService("C:\\Users\\davei\\AppData\\Roaming\\windash2\\widgets"));

                // Register core managers
                services.AddSingleton<WidgetManager>();

                // Register views
                services.AddTransient<ManagerWindow>();

                // Register TrayManager with factory for ManagerWindow
                services.AddSingleton(provider =>
                    new TrayManager(() => provider.GetRequiredService<ManagerWindow>(),
                    provider.GetRequiredService<WidgetManager>()));
            })
            .Build();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        try
        {
            var widgetManager = AppHost.Services.GetRequiredService<WidgetManager>();
            widgetManager.Initialize();

            // Start tray manager (creates tray icon)
            _trayManager = AppHost.Services.GetRequiredService<TrayManager>();

            // Do not show any main window on startup
            // Widgets will launch themselves, no taskbar or tray icon for them
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Unhandled exception during launch: " + ex);
        }
    }
}
