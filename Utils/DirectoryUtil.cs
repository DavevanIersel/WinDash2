using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WinDash2.Models;

namespace WinDash2.Utils;

public static class DirectoryUtil
{
    /// <summary>
    /// Returns the directory containing the settings config file. 
    /// Creates a new directory if it doesn't exist.
    /// </summary>
    public static string GetSettingsFolder()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinDash2"
#if DEBUG
            , "dev"
#endif
        );

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    /// <summary>
    /// Returns the directory for widgets, using settings if set.
    /// Falls back to default Appsettings directory.
    /// </summary>
    public static string GetWidgetsFolder(Settings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var defaultPath = Path.Combine(GetSettingsFolder(),
#if DEBUG
            "dev",
#endif
            "widgets"
        );

        var resolvedWidgetPath = settings.WidgetsFolderPath;

        // Validate that no file exists at the configured path. This check exists because a directory path may *look* like a file path
        // (e.g. "C:\WinDash\widgets.txt" is still a perfectly valid directory path on Windows).
        // However, if a file already exists at that location, we cannot create a directory there so we fall back to the default path.
        if (string.IsNullOrWhiteSpace(resolvedWidgetPath) || File.Exists(resolvedWidgetPath))
         {
            resolvedWidgetPath = defaultPath;
        }

        if (!Directory.Exists(resolvedWidgetPath))
        {
            Directory.CreateDirectory(resolvedWidgetPath);
        }

        return resolvedWidgetPath;
    }

    public static void OpenDirectoryInFileExplorer(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            Debug.WriteLine("OpenDirectoryInFileExplorer: Provided path is null or whitespace.");
            return;
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OpenDirectoryInFileExplorer: Invalid path '{path}': {ex.Message}");
            return;
        }

        if (!Directory.Exists(fullPath))
        {
            Debug.WriteLine($"OpenDirectoryInFileExplorer: Directory does not exist at path '{fullPath}'.");
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = fullPath,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    public async static Task<Windows.Storage.StorageFolder> PickFolderAsync(IntPtr hwnd, string allowedFileTypes)
    {
        var folderPicker = new Windows.Storage.Pickers.FolderPicker
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
        };
        folderPicker.FileTypeFilter.Add(allowedFileTypes);

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        return await folderPicker.PickSingleFolderAsync();
    }
}
