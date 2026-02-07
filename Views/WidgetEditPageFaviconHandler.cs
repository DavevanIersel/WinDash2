using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WinDash2.Core;
using WinDash2.Models;
using WinDash2.Utils;

namespace WinDash2.Views;

/// <summary>
/// Handles all favicon-related operations for WidgetEditPage.
/// </summary>
internal class WidgetEditPageFaviconHandler(
    Image faviconImage,
    ProgressRing faviconSpinner,
    TextBox nameTextBox,
    TextBlock customFaviconFilename,
    FrameworkElement customFaviconIndicator,
    Button deleteCustomFaviconButton,
    string widgetsRootPath)
{
    private readonly Image _faviconImage = faviconImage;
    private readonly ProgressRing _faviconSpinner = faviconSpinner;
    private readonly TextBox _nameTextBox = nameTextBox;
    private readonly TextBlock _customFaviconFilename = customFaviconFilename;
    private readonly FrameworkElement _customFaviconIndicator = customFaviconIndicator;
    private readonly Button _deleteCustomFaviconButton = deleteCustomFaviconButton;
    private readonly string _widgetsRootPath = widgetsRootPath;

    private const int FaviconFetchDelayMs = 500;

    private Stream? _pendingCustomFaviconStream;
    private CoreWebView2? _coreWebView;
    private Widget? _currentWidget;

    public Stream? PendingFaviconStream { get; private set; }

    /// <summary>
    /// Attaches to WebView2 navigation events to fetch favicon when pages load.
    /// </summary>
    public void AttachToWebView(CoreWebView2 coreWebView, Widget widget)
    {
        // Unsubscribe from previous instance if any
        if (_coreWebView != null)
        {
            _coreWebView.NavigationCompleted -= OnNavigationCompleted;
        }

        _coreWebView = coreWebView;
        _currentWidget = widget;
        _coreWebView.NavigationCompleted += OnNavigationCompleted;
    }

    /// <summary>
    /// Resets favicon state for new navigation.
    /// </summary>
    public void ResetForNewNavigation()
    {
        PendingFaviconStream?.Dispose();
        PendingFaviconStream = null;
        ShowSpinner();
        HideFavicon();
    }

    /// <summary>
    /// Event handler that fetches favicon when navigation completes.
    /// </summary>
    private async void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (!e.IsSuccess || _coreWebView == null || _currentWidget == null)
        {
            HideSpinner();
            HideFavicon();
            return;
        }

        // Skip if custom favicon is set
        if (!string.IsNullOrEmpty(_currentWidget.CustomFaviconPath))
        {
            HideSpinner();
            return;
        }

        try
        {
            // Wait a moment for favicon to be available
            await Task.Delay(FaviconFetchDelayMs);

            var faviconUri = _coreWebView.FaviconUri;
            if (string.IsNullOrEmpty(faviconUri))
                return;

            var stream = await _coreWebView.GetFaviconAsync(CoreWebView2FaviconImageFormat.Png);
            if (stream != null && stream.Size > 0)
            {
                using var ms = new MemoryStream();
                await stream.AsStreamForRead().CopyToAsync(ms);
                var bytes = ms.ToArray();

                if (bytes.Length > 100)
                {
                    PendingFaviconStream?.Dispose();
                    PendingFaviconStream = new MemoryStream(bytes);
                    await DisplayAsync(PendingFaviconStream);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to fetch favicon: {ex.Message}");
        }
        finally
        {
            HideSpinner();
            if (PendingFaviconStream == null)
            {
                HideFavicon();
            }
        }
    }

    /// <summary>
    /// Loads existing favicon from file system.
    /// </summary>
    public async Task LoadExistingAsync(Widget widget)
    {
        try
        {
            var bitmap = await FaviconUtil.LoadFaviconAsync(widget, _widgetsRootPath);
            
            if (bitmap != null)
            {
                _faviconImage.Source = bitmap;
                ShowFavicon();
            }
            else
            {
                HideFavicon();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load existing favicon: {ex.Message}");
            HideFavicon();
        }
    }

    /// <summary>
    /// Displays a favicon stream in the UI.
    /// </summary>
    private async Task DisplayAsync(Stream faviconStream)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            faviconStream.Position = 0;
            await faviconStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var bitmap = new BitmapImage();
            using (var randomAccessStream = memoryStream.AsRandomAccessStream())
            {
                await bitmap.SetSourceAsync(randomAccessStream);
            }

            _faviconImage.Source = bitmap;
            ShowFavicon();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to display favicon: {ex.Message}");
            HideFavicon();
        }
    }

    /// <summary>
    /// Opens file picker to browse for custom favicon and updates UI.
    /// </summary>
    public async Task BrowseForCustomFaviconAsync(Widget widget)
    {
        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            var hwnd = WindowUtil.GetActiveWindowHandle<TrayManager>(trayManager => trayManager.ActiveManagerWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            foreach (var ext in new[] { ".ico", ".png", ".jpg", ".jpeg" })
            {
                picker.FileTypeFilter.Add(ext);
            }

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using var stream = await file.OpenStreamForReadAsync();
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                _pendingCustomFaviconStream?.Dispose();
                _pendingCustomFaviconStream = memoryStream;

                await DisplayAsync(memoryStream);
                UpdateCustomFaviconIndicator(widget);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load custom favicon: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes custom favicon and reloads regular favicon.
    /// </summary>
    public async Task DeleteCustomFaviconAsync(Widget widget, WidgetManager widgetManager)
    {
        try
        {
            _pendingCustomFaviconStream?.Dispose();
            _pendingCustomFaviconStream = null;

            FaviconUtil.DeleteCustomFavicon(widget, _widgetsRootPath);
            widgetManager.SaveWidget(widget, false);

            UpdateCustomFaviconIndicator(widget);
            await LoadExistingAsync(widget);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to delete custom favicon: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves pending favicon streams to disk.
    /// </summary>
    public async Task SavePendingFaviconsAsync(Widget widget)
    {
        if (PendingFaviconStream != null)
        {
            try
            {
                PendingFaviconStream.Position = 0;
                await FaviconUtil.SaveFaviconAsync(widget, PendingFaviconStream, _widgetsRootPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save favicon: {ex.Message}");
            }
            finally
            {
                PendingFaviconStream?.Dispose();
                PendingFaviconStream = null;
            }
        }

        if (_pendingCustomFaviconStream != null)
        {
            try
            {
                _pendingCustomFaviconStream.Position = 0;
                await FaviconUtil.SaveCustomFaviconAsync(widget, _pendingCustomFaviconStream, _widgetsRootPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save custom favicon: {ex.Message}");
            }
            finally
            {
                _pendingCustomFaviconStream?.Dispose();
                _pendingCustomFaviconStream = null;
            }
        }
    }

    /// <summary>
    /// Updates custom favicon indicator visibility and text.
    /// </summary>
    public void UpdateCustomFaviconIndicator(Widget widget)
    {
        var hasCustomFavicon = !string.IsNullOrWhiteSpace(widget.CustomFaviconPath) || _pendingCustomFaviconStream != null;
        
        _customFaviconIndicator.Visibility = hasCustomFavicon ? Visibility.Visible : Visibility.Collapsed;
        _deleteCustomFaviconButton.Visibility = hasCustomFavicon ? Visibility.Visible : Visibility.Collapsed;

        if (hasCustomFavicon)
        {
            _customFaviconFilename.Text = widget.CustomFaviconPath ?? "custom-favicon.ico";
        }
    }

    /// <summary>
    /// Cleans up pending favicon streams and event handlers.
    /// </summary>
    public void Cleanup()
    {
        if (_coreWebView != null)
        {
            _coreWebView.NavigationCompleted -= OnNavigationCompleted;
            _coreWebView = null;
        }

        _currentWidget = null;
        PendingFaviconStream?.Dispose();
        PendingFaviconStream = null;
        _pendingCustomFaviconStream?.Dispose();
        _pendingCustomFaviconStream = null;
    }

    private void ShowSpinner()
    {
        _faviconSpinner.Visibility = Visibility.Visible;
        _faviconImage.Visibility = Visibility.Collapsed;
        _nameTextBox.Padding = new Thickness(36, 8, 12, 8);
    }

    private void HideSpinner()
    {
        _faviconSpinner.Visibility = Visibility.Collapsed;
    }

    private void ShowFavicon()
    {
        _faviconSpinner.Visibility = Visibility.Collapsed;
        _faviconImage.Visibility = Visibility.Visible;
        _nameTextBox.Padding = new Thickness(36, 8, 12, 8);
    }

    private void HideFavicon()
    {
        _faviconImage.Visibility = Visibility.Collapsed;
        _faviconImage.Source = null;
    }
}
