using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using WinDash2.Models;

namespace WinDash2.WidgetOptions;

// Will likely be removed. Due to running WinDash2 on the local Edge chromium runtime we can make use of the built-in permissions and session.
// This was a custom solution we needed for the Electron implmemtation. There are very minor positives for example when sharing widgets, or reinstalling Edge, but also potential security vulnerabilities.
public class PermissionsOption : IWidgetOption
{
    public void Apply(Widget widget, CoreWebView2 coreWebView2)
    {
        coreWebView2.PermissionRequested += (sender, args) =>
        {
            if (TryMapPermission(args.PermissionKind, out var permissionKey))
            {
                if (widget.Permissions != null && widget.Permissions.TryGetValue(permissionKey, out var allow))
                {
                    Debug.WriteLine(allow);
                    args.State = allow == true
                        ? CoreWebView2PermissionState.Allow
                        : CoreWebView2PermissionState.Deny;
                    args.Handled = true;
                    return;
                }
            }

            args.State = CoreWebView2PermissionState.Default; // Usually results in a UI prompt, or a silent deny dependent on browser behaviour
            // TODO: If the user is prompted to save their permission preferences, ideally this is saved in the widget data. This would keep the config files and browser data in sync.
            args.Handled = true;
        };
    }

    // TODO: Remove permission. Use CoreWebView2PermissionKind everywhere. Check if all functionality will still be available.
    private static bool TryMapPermission(CoreWebView2PermissionKind kind, out Permission permission)
    {
        permission = kind switch
        {
            CoreWebView2PermissionKind.Geolocation => Permission.Geolocation,
            CoreWebView2PermissionKind.ClipboardRead => Permission.ClipboardRead,
            CoreWebView2PermissionKind.Notifications => Permission.Notifications,
            CoreWebView2PermissionKind.Camera => Permission.Media,
            CoreWebView2PermissionKind.Microphone => Permission.Media,
            CoreWebView2PermissionKind.UnknownPermission => Permission.Unknown,
            CoreWebView2PermissionKind.FileReadWrite => Permission.FileSystem,
            CoreWebView2PermissionKind.MidiSystemExclusiveMessages => Permission.MidiSysex,
            CoreWebView2PermissionKind.WindowManagement => Permission.WindowManagement,
                    _ => default
        };

        return Enum.IsDefined(typeof(Permission), permission);
    }
}