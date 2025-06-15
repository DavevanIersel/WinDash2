using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WinDash2.Models;

public class Widget
{
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
    public bool? TouchEnabled { get; set; }                                  // Indicates whether touch controls are simulated
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }                                       // Toggle widget on or off
    [JsonPropertyName("customUserAgent")]
    public List<UserAgentMapping>? CustomUserAgent { get; set; } = [];       // Mapping of domains to custom user agents(Some widget will make calls to different websites, which might require different user agents. CloudFlare or Google Login requests for example)
    [JsonPropertyName("permissions")]
    public Dictionary<Permission, bool?> Permissions { get; set; } = new Dictionary<Permission, bool?>();    // Mapping of permissions a widget may ask for which you want to always accept/decline. (Example: Geolocation)
    [JsonPropertyName("customScript")]
    public string? CustomScript { get; set; } = string.Empty;                // A custom JS script you want to be loaded on the widget webpage
    [JsonPropertyName("devTools")]
    public bool? DevTools { get; set; }                                      // Enable developer tools for this widget
    [JsonPropertyName("forceInCurrentTab")]
    public List<string>? ForceInCurrentTab { get; set; } = [];               // Paths forced into widget tab instead of separate tab
    [JsonIgnore]
    public string? FileName { get; set; }                                   // The filename for the widgetConfig, filled by WidgetFileSystemService
    [JsonIgnore]
    public Guid IdOrThrow => Id ?? throw new InvalidOperationException("Widget Id has not been assigned yet.");
}
