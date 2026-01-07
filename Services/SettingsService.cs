using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WinDash2.Models;

namespace WinDash2.Services;

public class SettingsService
{
    private readonly string _settingsFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private Settings _settings;

    public SettingsService(string settingsFolderPath)
    {
        ArgumentNullException.ThrowIfNull(settingsFolderPath);

        if (!Directory.Exists(settingsFolderPath))
        {
            Debug.WriteLine($"Directory '{settingsFolderPath}' not found. Creating...");
            Directory.CreateDirectory(settingsFolderPath);
        }

        _settingsFilePath = Path.Combine(settingsFolderPath, "settings.json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        _settings = LoadSettings();
    }

    private Settings LoadSettings()
    {
        if (!File.Exists(_settingsFilePath))
        {
            Debug.WriteLine($"Settings file not found at '{_settingsFilePath}'. Creating default settings.");
            var defaultSettings = new Settings();
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<Settings>(json, _jsonOptions);
            
            if (settings != null)
            {
                Debug.WriteLine($"Loaded settings from '{_settingsFilePath}'");
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

    public Settings GetSettings()
    {
        return _settings;
    }

    public void SaveSettings(Settings settings)
    {
        try
        {
            _settings = settings;
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(_settingsFilePath, json);
            Debug.WriteLine($"Settings saved to '{_settingsFilePath}'");
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
        _settings.GridSize = Math.Max(1, Math.Min(500, gridSize)); // Clamp between 1 and 500
        SaveSettings(_settings);
    }
}
