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
            })
            .Build();
    }

    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        try
        {
            var widgetManager = AppHost.Services.GetRequiredService<WidgetManager>();
            widgetManager.Initialize();

            var window = AppHost.Services.GetRequiredService<ManagerWindow>();
            _window = window;
            _window.Activate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Unhandled exception during launch: " + ex);
        }
    }
}
