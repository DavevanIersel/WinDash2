using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WinDash2.Models;


namespace WinDash2.Services;

public class WidgetFileSystemService
{
    private readonly string _widgetsFolderPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public WidgetFileSystemService(string widgetsFolderPath)
    {
        _widgetsFolderPath = widgetsFolderPath ?? throw new ArgumentNullException(nameof(widgetsFolderPath));

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public List<Widget> LoadAllWidgets()
    {
        if (!Directory.Exists(_widgetsFolderPath))
        {
            Debug.WriteLine($"Directory '{_widgetsFolderPath}' not found. Creating...");
            Directory.CreateDirectory(_widgetsFolderPath);
        }

        var widgetFiles = Directory.GetFiles(_widgetsFolderPath, "*.widget.json", SearchOption.AllDirectories);
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
                    widget.FileName = Path.GetRelativePath(_widgetsFolderPath, file);
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

    string ReadFileContent(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }


    public void SaveWidget(Widget widget)
    {
        ArgumentNullException.ThrowIfNull(widget.FileName);

        var widgetFilePath = Path.Combine(_widgetsFolderPath, widget.FileName);

        if (widgetFilePath != null)
        {
            using var stream = File.Create(widgetFilePath);
            JsonSerializer.SerializeAsync(stream, widget, _jsonOptions);
        }
        else
        {
            throw new FileNotFoundException($"Widget config with name '{widget.Name}' and path '{widgetFilePath}' not found.");
        }
    }

    public void DeleteWidget(Widget widget)
    {
        ArgumentNullException.ThrowIfNull(widget.FileName);

        var widgetFilePath = Path.Combine(_widgetsFolderPath, widget.FileName);

        if (widgetFilePath != null)
        {
            File.Delete(widgetFilePath);
        }
        else
        {
            throw new FileNotFoundException($"Widget config with name '{widget.Name}' and path '{widgetFilePath}' not found.");
        }
    }
}