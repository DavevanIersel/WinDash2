# WinDash 2

**WinDash 2** is a lightweight Windows desktop dashboard that lets you pin webpages directly to your desktop as resizable, movable widgets.

It is a complete rewrite of WinDash, focused on simplicity, performance, and native Windows integration.

-   **Web Widgets**: Embed any webpage as a desktop widget.
-   **Portable by Design**: No installer, no registry writes — just run the executable.
-   **Built for Windows**: Windows 10/11 only.
-   **Stack**: .NET 8, WinUI 3
  
---

## Usage

WinDash 2 is distributed as a **portable executable**.

1. Download the latest release from the GitHub Releases page
2. Extract the archive anywhere you like
3. Run `WinDash2.exe`

All configuration and data stay local to the application folder / appdata folder. You can move or delete it at any time without leftovers.

---

## Features

-   Embed arbitrary webpages as desktop widgets
-   Free positioning and resizing
-   Optional launch on Windows startup

---

## Building from Source

### Requirements

-   Windows 10 or 11
-   .NET 8 SDK
-   Visual Studio 2022 (recommended)

### Development / Test Build

Open the solution in Visual Studio or build via command line:

```powershell
dotnet build
```

### Creating Release Builds

**Build portable executables:**

```powershell
.\build-release.ps1 -Platform <x64|x86|ARM64|All>
```

Output: `bin\Release\portable\<platform>\`

**Package as distribution-ready ZIP files:**

```powershell
.\package-release.ps1 -Platform <x64|x86|ARM64|All>
```

Output: `release\WinDash2-portable-win-<platform>.zip`

**Platform options:**

-   `x64` - 64-bit Intel/AMD (most common)
-   `x86` - 32-bit systems
-   `ARM64` - ARM64 processors
-   `All` - Build/package all three architectures
