using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace WinDash2.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum KeyCode
{
    [EnumMember(Value = "F1")]
    F1,
    
    [EnumMember(Value = "F2")]
    F2,
    
    [EnumMember(Value = "F3")]
    F3,
    
    [EnumMember(Value = "F4")]
    F4,
    
    [EnumMember(Value = "F5")]
    F5,
    
    [EnumMember(Value = "F6")]
    F6,
    
    [EnumMember(Value = "F7")]
    F7,
    
    [EnumMember(Value = "F8")]
    F8,
    
    [EnumMember(Value = "F9")]
    F9,
    
    [EnumMember(Value = "F10")]
    F10,
    
    [EnumMember(Value = "F11")]
    F11,
    
    [EnumMember(Value = "F12")]
    F12,
    
    [EnumMember(Value = "Space")]
    Space,
    
    [EnumMember(Value = "Enter")]
    Enter,
    
    [EnumMember(Value = "Escape")]
    Escape
}