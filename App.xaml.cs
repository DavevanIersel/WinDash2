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
    public static IHost? AppHost { get; private set; }

    public App()
    {
        InitializeComponent();

        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<SettingsService>();
                services.AddSingleton<WidgetFileSystemService>();
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

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(AppHost);

        try
        {
            var widgetManager = AppHost.Services.GetRequiredService<WidgetManager>();
            widgetManager.Initialize();

            AppHost.Services.GetRequiredService<TrayManager>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Unhandled exception during launch: " + ex);
        }
    }
}