using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.UI.Xaml;
using WinDash2.Core;
using WinDash2.Services;
using WinDash2.Views;
using System.IO;

namespace WinDash2;

public partial class App : Application
{
    private TrayManager? _trayManager;
    public static IHost AppHost { get; private set; }
    static readonly string WIDGETS_PATH = Path.Combine(
       Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
       "WinDash2",
#if DEBUG
       "dev",
#endif
       "widgets"
   );
    
    static readonly string SETTINGS_PATH = Path.Combine(
       Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
       "WinDash2"
#if DEBUG
       , "dev"
#endif
   );

    public App()
    {
        InitializeComponent();

        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(provider =>
                    new WidgetFileSystemService(WIDGETS_PATH));
                services.AddSingleton(provider =>
                    new SettingsService(SETTINGS_PATH));
                services.AddSingleton<GridService>();
                services.AddSingleton<WidgetManager>();
                services.AddTransient<ManagerWindow>();
                services.AddTransient<WidgetLibraryPage>();
                services.AddTransient<WidgetEditPage>();
                services.AddTransient<SettingsPage>();
                services.AddSingleton(provider =>
                    new TrayManager(
                        () => provider.GetRequiredService<ManagerWindow>(),
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

            _trayManager = AppHost.Services.GetRequiredService<TrayManager>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Unhandled exception during launch: " + ex);
        }
    }
}
