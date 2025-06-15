using System.Runtime.Serialization;

namespace WinDash2.Models;

public enum Permission
{
    [EnumMember(Value = "clipboard-read")]
    ClipboardRead,

    [EnumMember(Value = "clipboard-sanitized-write")]
    ClipboardSanitizedWrite,

    [EnumMember(Value = "display-capture")]
    DisplayCapture,

    [EnumMember(Value = "fullscreen")]
    Fullscreen,

    [EnumMember(Value = "geolocation")]
    Geolocation,

    [EnumMember(Value = "idle-detection")]
    IdleDetection,

    [EnumMember(Value = "media")]
    Media,

    [EnumMember(Value = "mediaKeySystem")]
    MediaKeySystem,

    [EnumMember(Value = "midi")]
    Midi,

    [EnumMember(Value = "midiSysex")]
    MidiSysex,

    [EnumMember(Value = "notifications")]
    Notifications,

    [EnumMember(Value = "pointerLock")]
    PointerLock,

    [EnumMember(Value = "keyboardLock")]
    KeyboardLock,

    [EnumMember(Value = "openExternal")]
    OpenExternal,

    [EnumMember(Value = "speaker-selection")]
    SpeakerSelection,

    [EnumMember(Value = "storage-access")]
    StorageAccess,

    [EnumMember(Value = "top-level-storage-access")]
    TopLevelStorageAccess,

    [EnumMember(Value = "window-management")]
    WindowManagement,

    [EnumMember(Value = "unknown")]
    Unknown,

    [EnumMember(Value = "fileSystem")]
    FileSystem
}
