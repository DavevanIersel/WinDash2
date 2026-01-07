using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using WinDash2.Views;

namespace WinDash2.Core;

internal class TrayManager : IDisposable
{
    public IRelayCommand OpenManagerCommand { get; }
    public IRelayCommand OpenSettingsCommand { get; }
    public IRelayCommand QuitCommand { get; }
    private readonly TaskbarIcon _trayIcon;
    private readonly Func<ManagerWindow> _managerWindowFactory;
    private readonly Func<SettingsWindow> _settingsWindowFactory;
    private ManagerWindow? _managerWindow;
    private SettingsWindow? _settingsWindow;
    private readonly WidgetManager _widgetManager;

    public TrayManager(Func<ManagerWindow> managerWindowFactory, Func<SettingsWindow> settingsWindowFactory, WidgetManager widgetManager)
    {
        _managerWindowFactory = managerWindowFactory;
        _settingsWindowFactory = settingsWindowFactory;
        _widgetManager = widgetManager;

        OpenManagerCommand = new RelayCommand(OpenManagerWindow);
        OpenSettingsCommand = new RelayCommand(OpenSettingsWindow);
        QuitCommand = new RelayCommand(QuitApplication);

        var menuFlyout = new MenuFlyout();

        menuFlyout.Items.Add(new MenuFlyoutItem
        {
            Text = "Open Manager",
            Command = OpenManagerCommand
        });

        menuFlyout.Items.Add(new MenuFlyoutItem
        {
            Text = "Settings",
            Command = OpenSettingsCommand
        });

        menuFlyout.Items.Add(new MenuFlyoutItem
        {
            Text = "Quit",
            Command = QuitCommand
        });

        _trayIcon = new TaskbarIcon
        {
            IconSource = new BitmapImage(new Uri("ms-appx:///Assets/logo96x96.ico")),
            ToolTipText = "WinDash2",
            ContextFlyout = menuFlyout
        };

        _trayIcon.ForceCreate();
    }

    private void OpenManagerWindow()
    {
        if (_managerWindow == null)
        {
            _managerWindow = _managerWindowFactory();
            _managerWindow.Closed += OnManagerClosed;
            _managerWindow.Activate();
        }
        else
        {
            _managerWindow.Activate();
        }

        _widgetManager.SetDraggable(true);
    }

    private void OnManagerClosed(object sender, WindowEventArgs args)
    {
        _managerWindow = null;
        _widgetManager.SetDraggable(false);
    }

    private void OpenSettingsWindow()
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = _settingsWindowFactory();
            _settingsWindow.Closed += OnSettingsClosed;
            _settingsWindow.Activate();
        }
        else
        {
            _settingsWindow.Activate();
        }
    }

    private void OnSettingsClosed(object sender, WindowEventArgs args)
    {
        _settingsWindow = null;
    }

    private void QuitApplication()
    {
        _managerWindow?.Close();
        _settingsWindow?.Close();
        _widgetManager.CloseAllWidgets();
    }

    public void Dispose()
    {
        _trayIcon.Dispose();
    }
}
