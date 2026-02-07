using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using WinDash2.Models;

namespace WinDash2.Utils;

public static class FaviconUtil
{
    /// <summary>
    /// Saves the favicon data to the widget's folder and updates the widget's FaviconPath property.
    /// Returns true if the favicon was successfully saved, false otherwise.
    /// </summary>
    public static Task<bool> SaveFaviconAsync(Widget widget, Stream faviconStream, string widgetsRootPath)
    {
        return SaveFaviconInternalAsync(widget, faviconStream, widgetsRootPath, isCustom: false);
    }

    /// <summary>
    /// Saves a custom favicon data to the widget's folder and updates the widget's CustomFaviconPath property.
    /// Returns true if the favicon was successfully saved, false otherwise.
    /// </summary>
    public static Task<bool> SaveCustomFaviconAsync(Widget widget, Stream faviconStream, string widgetsRootPath)
    {
        return SaveFaviconInternalAsync(widget, faviconStream, widgetsRootPath, isCustom: true);
    }

    /// <summary>
    /// Internal helper method to save favicon data.
    /// </summary>
    private static async Task<bool> SaveFaviconInternalAsync(Widget widget, Stream faviconStream, string widgetsRootPath, bool isCustom)
    {
        ArgumentNullException.ThrowIfNull(widget);
        ArgumentNullException.ThrowIfNull(faviconStream);
        ArgumentException.ThrowIfNullOrEmpty(widget.FileName);

        try
        {
            var fileName = isCustom ? "custom-favicon.ico" : "favicon.ico";
            var faviconPath = widget.GetFilePath(fileName, widgetsRootPath);

            using (var fileStream = new FileStream(faviconPath, FileMode.Create, FileAccess.Write))
            {
                await faviconStream.CopyToAsync(fileStream);
            }

            if (isCustom)
            {
                widget.CustomFaviconPath = fileName;
            }
            else
            {
                widget.FaviconPath = fileName;
            }
            
            Debug.WriteLine($"{(isCustom ? "Custom favicon" : "Favicon")} saved successfully for widget '{widget.Name}' at: {faviconPath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save {(isCustom ? "custom " : "")}favicon for widget '{widget.Name}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Deletes the custom favicon for the given widget.
    /// </summary>
    public static void DeleteCustomFavicon(Widget widget, string widgetsRootPath)
    {
        ArgumentNullException.ThrowIfNull(widget);

        if (string.IsNullOrWhiteSpace(widget.CustomFaviconPath))
        {
            return;
        }

        try
        {
            var faviconPath = widget.GetFilePath(widget.CustomFaviconPath, widgetsRootPath);
            
            if (File.Exists(faviconPath))
            {
                File.Delete(faviconPath);
            }

            widget.CustomFaviconPath = null;
            Debug.WriteLine($"Custom favicon deleted for widget '{widget.Name}'");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to delete custom favicon for widget '{widget.Name}': {ex.Message}");
        }
    }

    /// <summary>
    /// Loads the favicon as a BitmapImage for display in the UI.
    /// Prefers CustomFaviconPath if available, otherwise uses FaviconPath.
    /// Returns null if the favicon doesn't exist or cannot be loaded.
    /// </summary>
    public static async Task<BitmapImage?> LoadFaviconAsync(Widget widget, string widgetsRootPath)
    {
        ArgumentNullException.ThrowIfNull(widget);
        var faviconPath = widget.CustomFaviconPath ?? widget.FaviconPath;

        if (string.IsNullOrWhiteSpace(faviconPath))
        {
            return null;
        }

        try
        {
            var fullFaviconPath = widget.GetFilePath(faviconPath, widgetsRootPath);
            
            if (!File.Exists(fullFaviconPath))
            {
                return null;
            }

            var bitmap = new BitmapImage();
            using (var fileStream = File.OpenRead(fullFaviconPath))
            {
                var randomAccessStream = fileStream.AsRandomAccessStream();
                await bitmap.SetSourceAsync(randomAccessStream);
            }

            return bitmap;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load favicon for widget '{widget.Name}': {ex.Message}");
            return null;
        }
    }
}
