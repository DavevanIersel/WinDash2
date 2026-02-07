using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinDash2.Core;
using WinDash2.Models;
using WinDash2.Utils;
using WinDash2.Services;
using WinDash2.WidgetOptions;
using WinDash2.WidgetOptions.FunctionKeyActions;
using WinDash2.WidgetOptions.HideScrollbarOption;

namespace WinDash2.Views;

public class WidgetEditArgs
{
    public required Widget Widget { get; init; }
    public required WidgetManager WidgetManager { get; init; }
    public bool IsNewWidget { get; init; } = false;
}

public sealed partial class WidgetEditPage : Page
{
    private WidgetManager? _widgetManager;
    private WidgetFileSystemService? _widgetFileSystemService;
    private bool _isNewWidget;
    private Widget? _originalWidget;
    private WidgetEditPageFaviconHandler? _faviconHandler;
    private readonly DebouncedAction _debouncedPreviewUpdate = new(500);
    private string? _lastNavigatedUrl;
    private string? _lastNavigatedHtml;

    public Widget Widget { get; set; }
    public ObservableCollection<ForceInCurrentTabPattern> ForceInCurrentTabPatterns { get; } = new();

    private static readonly IWidgetOption[] Options =
    [
        new FunctionKeyActions(),
        new UserAgentOption(),
        new TouchOption(),
        new ForceInCurrentTabOption(),
        new HideScrollbarOption(),
        new MouseNavigationOption(),
    ];

    public WidgetEditPage()
    {
        this.InitializeComponent();
        this.Loaded += OnPageLoad;

        ArgumentNullException.ThrowIfNull(App.AppHost);
        _widgetFileSystemService = App.AppHost.Services.GetRequiredService<WidgetFileSystemService>();
    }

    private async void OnPageLoad(object sender, RoutedEventArgs e)
    {
        await PreviewWebView.EnsureCoreWebView2Async();

        ArgumentNullException.ThrowIfNull(_widgetFileSystemService);
        _faviconHandler = new WidgetEditPageFaviconHandler(
            NameFaviconImage,
            FaviconSpinner,
            NameTextBox,
            CustomFaviconFilename,
            CustomFaviconIndicator,
            DeleteCustomFaviconButton,
            _widgetFileSystemService.WidgetsFolderPath);

        // Load existing favicon for existing widgets
        if (!_isNewWidget)
        {
            await _faviconHandler.LoadExistingAsync(Widget);
            _faviconHandler.UpdateCustomFaviconIndicator(Widget);
            
            // Initialize navigation tracking to prevent unnecessary reset on first UpdatePreview
            _lastNavigatedUrl = Widget.Url;
            _lastNavigatedHtml = Widget.Html;
        }

        UpdateUrlHtmlFieldStates();
        await UpdatePreview();
    }

    private async Task UpdatePreview()
    {
        if (PreviewWebView.CoreWebView2 == null)
        {
            return;
        }

        foreach (var option in Options)
        {
            option.Apply(Widget, PreviewWebView, null);
        }

        PreviewWebView.Width = Widget.Width;
        PreviewWebView.Height = Widget.Height;

        if (!string.IsNullOrWhiteSpace(Widget.Html) || !string.IsNullOrWhiteSpace(Widget.Url))
        {
            if (_faviconHandler != null)
            {
                // Ensure handler is attached to WebView before navigation
                if (PreviewWebView.CoreWebView2 != null && Widget != null)
                {
                    _faviconHandler.AttachToWebView(PreviewWebView.CoreWebView2, Widget);
                }

                // Only reset favicon state if URL/HTML actually changed (not just width/height/etc)
                if (_lastNavigatedUrl != Widget.Url || _lastNavigatedHtml != Widget.Html)
                {
                    _faviconHandler.ResetForNewNavigation();
                    _lastNavigatedUrl = Widget.Url;
                    _lastNavigatedHtml = Widget.Html;
                }
            }

            PlaceholderPanel.Visibility = Visibility.Collapsed;
            PreviewWebView.Visibility = Visibility.Visible;

            try
            {
                await CustomWidgetLoader.LoadAsync(Widget, PreviewWebView);
            }
            catch (Exception ex)
            {
                // Show error in placeholder
                PreviewWebView.Visibility = Visibility.Collapsed;
                PlaceholderPanel.Visibility = Visibility.Visible;

                var stackPanel = PlaceholderPanel.Child as StackPanel;
                if (stackPanel != null && stackPanel.Children.Count >= 2)
                {
                    if (stackPanel.Children[1] is TextBlock titleBlock)
                        titleBlock.Text = "Error Loading Widget";
                    if (stackPanel.Children.Count >= 3 && stackPanel.Children[2] is TextBlock msgBlock)
                        msgBlock.Text = ex.Message;
                }
            }
        }
        else
        {
            // Show native placeholder when URL is empty or invalid
            PreviewWebView.Visibility = Visibility.Collapsed;
            PlaceholderPanel.Visibility = Visibility.Visible;
        }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is WidgetEditArgs args)
        {
            Widget = args.Widget;
            _widgetManager = args.WidgetManager;
            _isNewWidget = args.IsNewWidget;

            // Backup original widget state for cancel
            _originalWidget = Widget.Clone();

            Widget.PropertyChanged += Widget_PropertyChanged;
            TouchEnabledSwitch.IsOn = Widget.TouchEnabled.GetValueOrDefault();

            ForceInCurrentTabPatterns.Clear();
            if (Widget.ForceInCurrentTab != null)
            {
                foreach (var pattern in Widget.ForceInCurrentTab)
                {
                    ForceInCurrentTabPatterns.Add(new ForceInCurrentTabPattern { Pattern = pattern });
                }
            }

            UpdateUrlHtmlFieldStates();

            PageTitle.Text = _isNewWidget ? "Create Widget" : "Edit Widget";

            if (!_isNewWidget)
            {
                OpenFolderButton.Visibility = Visibility.Visible;
            }
        }
    }

    private void UpdateSaveButtonState()
    {
        var hasContent = !string.IsNullOrWhiteSpace(Widget.Url) || !string.IsNullOrWhiteSpace(Widget.Html);
        SaveButton.IsEnabled = !_isNewWidget || hasContent;
    }

    private async void BrowseFavicon_Click(object sender, RoutedEventArgs e)
    {
        if (_faviconHandler == null) return;
        await _faviconHandler.BrowseForCustomFaviconAsync(Widget);
    }

    private async void DeleteCustomFavicon_Click(object sender, RoutedEventArgs e)
    {
        if (_faviconHandler == null || _widgetManager == null) return;
        await _faviconHandler.DeleteCustomFaviconAsync(Widget, _widgetManager);
    }

    private void OpenWidgetFolder_Click(object sender, RoutedEventArgs e)
    {
        if (_widgetFileSystemService == null) return;

        try
        {
            var widgetFolder = Widget.GetFolderPath(_widgetFileSystemService.WidgetsFolderPath);
            DirectoryUtil.OpenDirectoryInFileExplorer(widgetFolder);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open widget folder: {ex.Message}");
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_widgetManager == null || _widgetFileSystemService == null) return;

        Widget.ForceInCurrentTab = ForceInCurrentTabPatterns
            .Select(p => p.Pattern)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        if (_isNewWidget)
        {
            var safeName = string.Join("_", Widget.Name.Split(Path.GetInvalidFileNameChars()));
            if (string.IsNullOrWhiteSpace(safeName))
                safeName = "widget";

            Widget.FileName = $"{safeName.ToLower()}{Widget.FullFileExtension}";
            Widget.Id ??= Guid.NewGuid();
        }

        if (_faviconHandler != null)
        {
            await _faviconHandler.SavePendingFaviconsAsync(Widget);
        }

        _debouncedPreviewUpdate.Cancel();

        _widgetManager.SaveWidget(Widget, true);
        Frame.GoBack();
    }

    private void Widget_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateSaveButtonState();

        // Don't update preview for properties that don't affect the display
        if (e.PropertyName is nameof(Widget.Name) or nameof(Widget.Enabled) or 
            nameof(Widget.X) or nameof(Widget.Y) or nameof(Widget.Id) or nameof(Widget.FileName))
        {
            return;
        }

        _debouncedPreviewUpdate.Execute(UpdatePreview);
    }

    private void TouchEnabledSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            Widget.TouchEnabled = toggleSwitch.IsOn;
            UpdatePreview();
        }
    }

    private void OnUrlOrHtmlChanged(object sender, TextChangedEventArgs e)
    {
        UpdateUrlHtmlFieldStates();
        _debouncedPreviewUpdate.Execute(UpdatePreview);
    }

    private void OnInputChanged(object sender, TextChangedEventArgs e)
    {
        _debouncedPreviewUpdate.Execute(UpdatePreview);
    }

    private void UpdateUrlHtmlFieldStates()
    {
        var hasUrl = !string.IsNullOrWhiteSpace(UrlTextBox.Text);
        var hasHtml = !string.IsNullOrWhiteSpace(HtmlTextBox.Text);

        UrlTextBox.IsEnabled = !hasHtml;
        HtmlTextBox.IsEnabled = !hasUrl;
    }

    private void AddUserAgent_Click(object sender, RoutedEventArgs e)
    {
        Widget.CustomUserAgent ??= [];
        Widget.CustomUserAgent.Add(new UserAgentMapping
        {
            Domain = "",
            UserAgent = ""
        });
        UpdateUserAgentsinEditor();
    }

    private void RemoveUserAgent_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is UserAgentMapping mapping)
        {
            Widget.CustomUserAgent?.Remove(mapping);
            UpdateUserAgentsinEditor();
        }
    }

    private void UpdateUserAgentsinEditor()
    {
        UserAgentsList.ItemsSource = null;
        UserAgentsList.ItemsSource = Widget.CustomUserAgent;
        UpdatePreview();
    }

    private void AddForceInCurrentTab_Click(object sender, RoutedEventArgs e)
    {
        ForceInCurrentTabPatterns.Add(new ForceInCurrentTabPattern { Pattern = "" });
        UpdatePreview();
    }

    private void RemoveForceInCurrentTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ForceInCurrentTabPattern pattern)
        {
            ForceInCurrentTabPatterns.Remove(pattern);
            UpdatePreview();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        _debouncedPreviewUpdate.Cancel();

        if (_originalWidget != null)
        {
            Widget.CopyFrom(_originalWidget);
        }

        _faviconHandler?.Cleanup();

        Frame.GoBack();
    }
}
