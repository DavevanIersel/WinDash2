using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace WinDash2.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum KeyboardEventType
{
    [EnumMember(Value = "keydown")]
    Keydown,
    
    [EnumMember(Value = "keyup")]
    Keyup
}