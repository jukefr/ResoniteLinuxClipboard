# LinuxClipboard

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod that provides Linux clipboard support for Resonite.

## GitHub

**Repository**: [https://github.com/jukefr/ResoniteLinuxClipboard](https://github.com/jukefr/ResoniteLinuxClipboard)

## Features

- **Linux support**: Uses `wl-copy` and `wl-paste` (from `wl-clipboard` package)
- **X11 fallback**: Uses `xclip` when Linux tools aren't available
- **Text clipboard**: Copy/paste text between Resonite and system clipboard
- **Image clipboard**: Copy/paste images (PNG format)
- **Configurable**: Mod configuration options via ResoniteModLoader
- **Error handling**: Robust error handling with timeouts for all clipboard operations

## Backends

| Backend | Tools Required | Status |
|---------|----------------|--------|
| Linux | `wl-copy`, `wl-paste` (wl-clipboard package) | Primary |
| X11 | `xclip` | Fallback |

## Installation

### Prerequisites

Install one of these from your package manager:

- **Linux sessions**: `wl-clipboard` package (provides `wl-copy` and `wl-paste`)
- **X11 sessions**: `xclip`

### For Steam Users

If you launch through Steam, you may need to copy the required binaries into your Resonite directory:

```bash
cp /usr/bin/wl-copy ~/.steam/steamapps/common/Resonite`
cp /usr/bin/wl-paste ~/.steam/steamapps/common/Resonite`
cp /usr/bin/xclip ~/.steam/steamapps/common/Resonite`
```

### Mod Installation

1. Build the mod: `dotnet build`
2. Copy the output DLL to your Resonite mods folder: `cp bin/Debug/net10.0/LinuxClipboard.dll ~/.steam/steamapps/common/Resonite/rml_mods/`

## Configuration

The mod supports the following configuration options (via ResoniteModLoader's config system):

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `ClipboardTimeoutMs` | `int` | `5000` | Timeout in milliseconds for clipboard operations (0 = no timeout) |

To configure, use ResoniteModLoader's configuration interface in-game.

## Building

### Prerequisites

- .NET 10.0 SDK
- Resonite installed (for ResoniteModLoader.dll, HarmonyLib.dll, etc.)

### Build Commands

```bash
# Build main mod`
dotnet build`

# Build and copy to Resonite mods folder`
dotnet build -t:Copy`
```

## Testing

The project includes comprehensive unit tests for core logic.

### Test Structure

```
ResoniteLinuxClipboard/
├── LinuxClipboard.Core/          # Pure logic (no Resonite dependencies)
│   ├── BackendDetector.cs        # Backend detection logic
│   └── IProcessRunner.cs         # Interface for mocking Process.Start
├── LinuxClipboard.Tests/        # xUnit test project
│   ├── BackendDetectionTests.cs  # Tests for backend detection
│   ├── ConfigurationTests.cs    # (placeholder - needs ResoniteModLoader)
│   ├── MimeTypeTests.cs        # (placeholder - needs refactoring)
│   └── InputValidationTests.cs  # (placeholder - needs ResoniteModLoader)
└── LinuxClipboard.cs           # Main mod class
```

## Hot Reload (Development)

Hot reload support is optional and only active when `ResoniteHotReloadLib.dll` is present.

To enable hot reload code paths while developing, build with:

```bash
dotnet build -p:DefineConstants="DEBUG;RML_HOTRELOAD"`
```

## Project Structure

```
ResoniteLinuxClipboard/
├── LinuxClipboard.Core/          # Core library (no Resonite dependencies)
│   ├── BackendDetector.cs
│   └── IProcessRunner.cs
├── LinuxClipboard.Tests/        # xUnit test project
│   ├── BackendDetectionTests.cs
│   ├── ConfigurationTests.cs   # (placeholder - needs ResoniteModLoader)
│   ├── MimeTypeTests.cs       # (placeholder - needs refactoring)
│   └── InputValidationTests.cs # (placeholder - needs ResoniteModLoader)
├── LinuxClipboard.cs          # Main mod class
├── LinuxClipboard.Core.csproj   # Core project file
├── LinuxClipboard.Tests.csproj # Test project file
├── LinuxClipboard.csproj        # Main project file
├── README.md
└── LICENSE                  # GNU GPL-3.0
```

## Improvements Made

- ✅ Made discovery mode configurable (default: off) via `EnableDiscovery` config`
- ✅ Added clipboard backend validation (check if tools exist before using)`
- ✅ Added comprehensive error handling (try-catch) to all clipboard operations`
- ✅ Added null checks for `Process.Start` returns`
- ✅ Fixed logging misuse (separated Info/Warn/Error levels)`
- ✅ Added configurable timeouts via `ClipboardTimeoutMs` config`
- ✅ Fixed MIME type handling (UTF8_STRING mapping only for X11)`
- ✅ Added input validation (null handling for SetText/SetBitmap)`
- ✅ Created LinuxClipboard.Core for testable logic`
- ✅ Added 24 unit tests (xUnit + NSubstitute)`

## License

This project is released under the **GNU GPL-3.0** - see [LICENSE](LICENSE) for details.
