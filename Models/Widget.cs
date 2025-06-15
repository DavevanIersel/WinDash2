using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace WinDash2.Models;

public class Widget : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _enabled;

    [JsonPropertyName("id")]
    public Guid? Id { get; set; }                                           // Generated from filename

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;                        // A name for the widget, this does not have to be unique

    [JsonPropertyName("html")]
    public string Html { get; set; } = string.Empty;                        // The html page that should be loaded for a custom widget

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;                         // The url which should be loaded

    [JsonPropertyName("x")]
    public int X { get; set; }                                              // Horizontal position of the upper left corner of the widget

    [JsonPropertyName("y")]
    public int Y { get; set; }                                              // Vertical position of the upper left corner of the widget

    [JsonPropertyName("width")]
    public int Width { get; set; }                                          // Width of the widget

    [JsonPropertyName("height")]
    public int Height { get; set; }                                         // Height of the widget

    [JsonPropertyName("touchEnabled")]
    public bool? TouchEnabled { get; set; }                                 // Indicates whether touch controls are simulated

    [JsonPropertyName("enabled")]
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled != value)
            {
                _enabled = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonPropertyName("customUserAgent")]
    public List<UserAgentMapping>? CustomUserAgent { get; set; } = [];      // Mapping of domains to custom user agents

    [JsonPropertyName("permissions")]
    public Dictionary<Permission, bool?> Permissions { get; set; } = new(); // Mapping of permissions to auto-accept/deny

    [JsonPropertyName("customScript")]
    public string? CustomScript { get; set; } = string.Empty;               // Custom JS script loaded on widget page

    [JsonPropertyName("devTools")]
    public bool? DevTools { get; set; }                                     // Enable developer tools

    [JsonPropertyName("forceInCurrentTab")]
    public List<string>? ForceInCurrentTab { get; set; } = [];              // Force these paths in widget tab

    [JsonIgnore]
    public string? FileName { get; set; }                                   // The filename for the widgetConfig

    [JsonIgnore]
    public Guid IdOrThrow => Id ?? throw new InvalidOperationException("Widget Id has not been assigned yet.");

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
