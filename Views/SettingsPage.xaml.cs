using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinDash2.Core;
using WinDash2.Models;
using WinDash2.Services;
using WinDash2.Utils;

namespace WinDash2.Views;

public sealed partial class SettingsPage : Page
{
    private readonly WidgetManager _widgetManager;
    private readonly SettingsService _settingsService;
    private readonly StartupService _startupService;

    public SettingsPage()
    {
        InitializeComponent();

        ArgumentNullException.ThrowIfNull(App.AppHost);
        _widgetManager = App.AppHost.Services.GetRequiredService<WidgetManager>();
        _settingsService = App.AppHost.Services.GetRequiredService<SettingsService>();
        _startupService = App.AppHost.Services.GetRequiredService<StartupService>();

        InitializeSettings();
    }

    private void InitializeSettings()
    {
        var settings = _settingsService.GetSettings();

        GridSizeNumberBox.Value = settings.GridSize;
        StartupToggleSwitch.IsOn = _startupService.IsStartupEnabled();
        WidgetFolderTextBox.Text = settings.WidgetsFolderPath;

        FreeDragRadioButton.IsChecked = settings.DragMode == DragMode.Free;
        GridBasedRadioButton.IsChecked = settings.DragMode == DragMode.GridBased;
    }

    private void OnDragModeChanged(object sender, RoutedEventArgs e)
    {
        var dragMode = FreeDragRadioButton.IsChecked == true
            ? DragMode.Free
            : DragMode.GridBased;

        _settingsService.UpdateDragMode(dragMode);
    }

    private void OnGridSizeChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(args.NewValue))
        {
            _settingsService.UpdateGridSize((int)args.NewValue);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void OnStartupToggled(object sender, RoutedEventArgs e)
    {
        var toggleSwitch = sender as ToggleSwitch;
        if (toggleSwitch != null)
        {
            _startupService.SetStartupEnabled(toggleSwitch.IsOn);
        }
    }

    private void OpenWidgetFolder_Click(object sender, RoutedEventArgs e)
    {
        var path = _settingsService.GetSettings().WidgetsFolderPath;
        DirectoryUtil.OpenDirectoryInFileExplorer(path);
    }

    private async void ChangeWidgetFolder_Click(object sender, RoutedEventArgs e)
    {
        var hwnd = WindowUtil.GetActiveWindowHandle<TrayManager>(trayManager => trayManager.ActiveManagerWindow);
        var pickedFolder = await DirectoryUtil.PickFolderAsync(hwnd, "*");

        if (pickedFolder != null)
        {
            var settings = _settingsService.GetSettings();
            settings.WidgetsFolderPath = pickedFolder.Path;
            _settingsService.SaveSettings(settings);

            WidgetFolderTextBox.Text = pickedFolder.Path;

            _widgetManager.ReloadWidgets();
        }
    }
}
