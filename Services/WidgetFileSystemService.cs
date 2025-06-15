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

        // Fix: Replace the incorrect method call with the correct Dahomey.Json extension method
        //_jsonOptions.GetTypeRegistry()
        //    .RegisterEnumConverter<Permission>();
    }

    public async Task<List<Widget>> LoadAllWidgetsAsync()
    {
        if (!Directory.Exists(_widgetsFolderPath))
        {
            Debug.WriteLine($"Directory '{_widgetsFolderPath}' not found. Creating...");
            Directory.CreateDirectory(_widgetsFolderPath);
        }

        var widgetFiles = Directory.GetFiles(_widgetsFolderPath, "*.widget.json");
        Console.WriteLine($"Found {widgetFiles.Length} widget files");

        var widgets = new List<Widget>();

        //foreach (var file in widgetFiles)
        //{
        //    try
        //    {
        //        var fileInfo = new FileInfo(file);
        //        Debug.WriteLine($"Reading file: {file} ({fileInfo.Length} bytes)");

        //        var readTask = File.ReadAllTextAsync(file);
        //        if (await Task.WhenAny(readTask, Task.Delay(2000)) != readTask)
        //        {
        //            throw new TimeoutException($"Timed out reading file: {file}");
        //        }

        //        var json = readTask.Result;
        //        var widget = JsonSerializer.Deserialize<Widget>(json, _jsonOptions);

        //        if (widget != null)
        //        {
        //            widget.Id ??= Guid.NewGuid();
        //            widget.FileName = Path.GetFileName(file);
        //            widgets.Add(widget);
        //            Debug.WriteLine($"Loaded widget: {widget.Name} ({widget.Id})");
        //        }
        //        else
        //        {
        //            Debug.WriteLine($"Null widget from: {file}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Error reading '{file}': {ex}");
        //    }
        //}
        Widget s = new Widget
        {
            Id = Guid.NewGuid(),
            Name = "Spotify",
            Url = "https://open.spotify.com",
            Enabled = false,
            X = 3295,
            Y = 208,
            Width = 832,
            Height = 464,
            TouchEnabled = true
        };

        Widget s2 = new Widget
        {
            Id = Guid.NewGuid(),
            Name = "Marktplaats",  // You might want to change this to "Marktplaats" for clarity
            Url = "https://www.marktplaats.nl/",
            Enabled = false,
            X = 2572,
            Y = 207,
            Width = 722,
            Height = 1008,
            TouchEnabled = true
        };
        return new List<Widget> { s, s2 };



        Debug.WriteLine($"Returning {widgets.Count} widgets");
        return widgets;
    }

    public async Task SaveWidgetAsync(Widget widget)
    {
        ArgumentNullException.ThrowIfNull(widget.FileName);

        var widgetFilePath = Path.Combine(_widgetsFolderPath, widget.FileName);

        if (widgetFilePath != null)
        {
            using var stream = File.Create(widgetFilePath);
            await JsonSerializer.SerializeAsync(stream, widget, _jsonOptions);
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