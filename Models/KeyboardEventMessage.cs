using System.Text.Json.Serialization;

namespace WinDash2.Models;

public class KeyboardEventMessage
{
    [JsonPropertyName("type")]
    public KeyboardEventType Type { get; set; }
    
    [JsonPropertyName("code")]
    public KeyCode Code { get; set; }
    
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}