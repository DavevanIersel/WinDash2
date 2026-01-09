using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WinDash2.Models;
using WinDash2.Services;

namespace WinDash2.Views;

public sealed partial class SettingsPage : Page
{
    private SettingsService? _settingsService;
    private GridService? _gridService;
    private StartupService? _startupService;

    public SettingsPage()
    {
        InitializeComponent();
        _startupService = new StartupService();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var parameters = e.Parameter as (SettingsService, GridService)?;
        if (parameters.HasValue)
        {
            _settingsService = parameters.Value.Item1;
            _gridService = parameters.Value.Item2;

            var settings = _settingsService.GetSettings();
            
            if (settings.DragMode == DragMode.Free)
            {
                FreeDragRadioButton.IsChecked = true;
            }
            else
            {
                GridBasedRadioButton.IsChecked = true;
            }

            GridSizeNumberBox.Value = settings.GridSize;
            
            // Load startup setting
            if (_startupService != null)
            {
                StartupToggleSwitch.IsOn = _startupService.IsStartupEnabled();
            }
        }
    }

    private void OnDragModeChanged(object sender, RoutedEventArgs e)
    {
        if (_settingsService == null) return;

        var dragMode = FreeDragRadioButton.IsChecked == true 
            ? DragMode.Free 
            : DragMode.GridBased;

        _settingsService.UpdateDragMode(dragMode);
    }

    private void OnGridSizeChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (_settingsService == null) return;
        
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
        if (_startupService == null) return;
        
        var toggleSwitch = sender as ToggleSwitch;
        if (toggleSwitch != null)
        {
            _startupService.SetStartupEnabled(toggleSwitch.IsOn);
        }
    }
}
