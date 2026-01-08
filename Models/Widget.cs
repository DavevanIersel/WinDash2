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
    private string _name = string.Empty;
    private string _url = string.Empty;
    private string _html = string.Empty;
    private int _width;
    private int _height;

    /// <summary>
    /// Unique identifier for the widget, typically generated from the filename.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }

    /// <summary>
    /// A display name for the widget. This does not have to be unique.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// The HTML page that should be loaded for a custom widget.
    /// </summary>
    [JsonPropertyName("html")]
    public string Html
    {
        get => _html;
        set => SetProperty(ref _html, value);
    }

    /// <summary>
    /// The URL which should be loaded in the widget.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    /// <summary>
    /// Horizontal position of the upper left corner of the widget.
    /// </summary>
    [JsonPropertyName("x")]
    public int X { get; set; }

    /// <summary>
    /// Vertical position of the upper left corner of the widget.
    /// </summary>
    [JsonPropertyName("y")]
    public int Y { get; set; }

    /// <summary>
    /// Width of the widget in pixels.
    /// </summary>
    [JsonPropertyName("width")]
    public int Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    /// <summary>
    /// Height of the widget in pixels.
    /// </summary>
    [JsonPropertyName("height")]
    public int Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    /// <summary>
    /// Indicates whether touch controls are simulated for this widget.
    /// </summary>
    [JsonPropertyName("touchEnabled")]
    public bool? TouchEnabled { get; set; }

    /// <summary>
    /// Indicates whether the widget is enabled (visible/active).
    /// </summary>
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

    /// <summary>
    /// List of domain-to-user-agent mappings for customizing the user agent per domain.
    /// </summary>
    [JsonPropertyName("customUserAgent")]
    public List<UserAgentMapping>? CustomUserAgent { get; set; } = [];

    /// <summary>
    /// Custom JavaScript code to be injected and executed on the widget page.
    /// </summary>
    [JsonPropertyName("customScript")]
    public string? CustomScript { get; set; } = string.Empty;

    /// <summary>
    /// Enables or disables developer tools for this widget.
    /// </summary>
    [JsonPropertyName("devTools")]
    public bool? DevTools { get; set; }

    /// <summary>
    /// List of URL patterns that, when matched, will force navigation to occur in the current widget/tab
    /// instead of opening a new widget window. Supports wildcards using '*' anywhere in the pattern.
    /// - Use "*" as the only entry to force all URLs to open in the current tab.
    /// - Patterns are matched against the full URL using wildcard logic:
    ///     - "*" matches any sequence of characters (e.g., "/foo/*/bar" matches "/foo/123/bar").
    ///     - Multiple patterns can be specified; if any pattern matches, the navigation is forced in-tab.
    /// Example:
    ///   "forceInCurrentTab": ["/webplayer", "*/foo*", "/bar/*/baz"]
    ///   "forceInCurrentTab": ["*"] // force all URLs in current tab
    /// </summary>
    [JsonPropertyName("forceInCurrentTab")]
    public List<string>? ForceInCurrentTab { get; set; } = [];

    /// <summary>
    /// The filename for the widget configuration file. Not serialized.
    /// </summary>
    [JsonIgnore]
    public string? FileName { get; set; }

    /// <summary>
    /// Returns the widget's ID or throws if it is not assigned. Not serialized.
    /// </summary>
    [JsonIgnore]
    public Guid IdOrThrow => Id ?? throw new InvalidOperationException("Widget Id has not been assigned yet.");

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        return false;
    }
}
