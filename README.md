# WinDash 2

**WinDash 2** is a lightweight Windows desktop dashboard that lets you pin webpages directly to your desktop as resizable, movable widgets.

It is a complete rewrite of WinDash, focused on simplicity, performance, and native Windows integration.

-   **Web Widgets**: Embed any webpage as a desktop widget.
-   **Portable by Design**: No installer, no registry writes — just run the executable.
-   **Built for Windows**: Windows 10/11 only.
-   **Stack**: .NET 8, WinUI 3

  <img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/26120621-1d88-427c-974b-7d73b571d92e" />
  <img width="1905" height="1011" alt="image" src="https://github.com/user-attachments/assets/0df04c3d-e937-45f1-87c9-b1c9d187f75e" />
  <img width="1899" height="1007" alt="image" src="https://github.com/user-attachments/assets/304d0d81-a712-41fc-9fb1-44fd786d1ffb" />
  <img width="1903" height="755" alt="image" src="https://github.com/user-attachments/assets/da6d153e-52a4-4aaf-b502-9c62b2cf7862" />

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
