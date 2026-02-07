using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WinDash2.Models;
using WinDash2.Utils;

namespace WinDash2.Services;

public class WidgetFileSystemService
{
    private readonly SettingsService _settingsService;
    private readonly JsonSerializerOptions _jsonOptions;

    public string WidgetsFolderPath => DirectoryUtil.GetWidgetsFolder(_settingsService.GetSettings());

    public WidgetFileSystemService(SettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public List<Widget> LoadAllWidgets()
    {
        var path = WidgetsFolderPath;

        Debug.WriteLine($"Widget directory: '{path}'");

        if (!Directory.Exists(path))
        {
            Debug.WriteLine($"Directory '{path}' not found. Creating...");
            Directory.CreateDirectory(path);
        }

        var widgetFiles = Directory.GetFiles(path, Widget.FileSearchPattern, SearchOption.AllDirectories);
        var widgets = new List<Widget>();

        foreach (var file in widgetFiles)
        {
            try
            {
                var fileInfo = new FileInfo(file);
                Debug.WriteLine($"Reading file: {file} ({fileInfo.Length} bytes)");

                var json = ReadFileContent(file);
                var widget = JsonSerializer.Deserialize<Widget>(json, _jsonOptions);

                if (widget != null)
                {
                    widget.Id ??= Guid.NewGuid();
                    widget.FileName = Path.GetRelativePath(path, file);
                    widgets.Add(widget);
                    Debug.WriteLine($"Loaded widget: {widget.Name} ({widget.Id})");
                }
                else
                {
                    Debug.WriteLine($"Null widget from: {file}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading '{file}': {ex}");
            }
        }

        Debug.WriteLine($"Returning {widgets.Count} widgets");
        return widgets;
    }

    private static string ReadFileContent(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public void SaveWidget(Widget widget)
    {
        ArgumentNullException.ThrowIfNull(widget.FileName);

        var widgetFilePath = GetWidgetFilePath(widget);
        using var stream = File.Create(widgetFilePath);
        JsonSerializer.Serialize(stream, widget, _jsonOptions);
    }

    public void DeleteWidget(Widget widget)
    {
        ArgumentNullException.ThrowIfNull(widget.FileName);

        var widgetFilePath = GetWidgetFilePath(widget);

        if (File.Exists(widgetFilePath))
        {
            File.Delete(widgetFilePath);
        }
        else
        {
            throw new FileNotFoundException($"Cannot delete widget: file not found at path '{widgetFilePath}'.");
        }
    }

    /// <summary>
    /// Gets the absolute path to a file within the widget's folder.
    /// </summary>
    private string GetAbsolutePath(Widget widget, string relativePath)
    {
        var widgetFolder = widget.GetFolderPath(WidgetsFolderPath);
        if (!Directory.Exists(widgetFolder))
        {
            Directory.CreateDirectory(widgetFolder);
        }
        return Path.Combine(widgetFolder, relativePath);
    }

    /// <summary>
    /// Gets the absolute path to the widget's JSON file.
    /// </summary>
    private string GetWidgetFilePath(Widget widget)
    {
        ArgumentNullException.ThrowIfNull(widget.FileName);
        var fileName = Path.GetFileName(widget.FileName);
        return GetAbsolutePath(widget, fileName);
    }
}