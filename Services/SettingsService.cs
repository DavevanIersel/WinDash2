using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WinDash2.Models;
using WinDash2.Utils;

namespace WinDash2.Services;

public class SettingsService
{
    private readonly JsonSerializerOptions _jsonOptions;
    private Settings _settings;
    public Settings GetSettings() => _settings;
    private string SettingsFilePath => Path.Combine(DirectoryUtil.GetSettingsFolder(), "settings.json");

    public SettingsService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        _settings = LoadSettings();
    }
    private Settings LoadSettings()
    {
        var path = SettingsFilePath;

        var folder = Path.GetDirectoryName(path)!;
        if (!Directory.Exists(folder))
        {
            Debug.WriteLine($"Directory '{folder}' not found. Creating...");
            Directory.CreateDirectory(folder);
        }

        if (!File.Exists(path))
        {
            Debug.WriteLine($"Settings file not found at '{path}'. Creating default settings.");
            var defaultSettings = new Settings
            {
                WidgetsFolderPath = Path.Combine(folder, "widgets")
            };
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<Settings>(json, _jsonOptions);

            if (settings != null)
            {
                Debug.WriteLine($"Loaded settings from '{path}'");
                return settings;
            }
            else
            {
                Debug.WriteLine($"Failed to deserialize settings. Using defaults.");
                return new Settings();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading settings file: {ex.Message}");
            return new Settings();
        }
    }

    public void SaveSettings(Settings settings)
    {
        try
        {
            _settings = settings;
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(SettingsFilePath, json);
            Debug.WriteLine($"Settings saved to '{SettingsFilePath}'");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    public void UpdateDragMode(DragMode dragMode)
    {
        _settings.DragMode = dragMode;
        SaveSettings(_settings);
    }

    public void UpdateGridSize(int gridSize)
    {
        _settings.GridSize = gridSize;
        SaveSettings(_settings);
    }
}