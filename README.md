# WaylandClipboard

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod that provides Wayland clipboard support for Resonite on Linux.

## Features

- **Wayland support**: Uses `wl-copy` and `wl-paste` (from `wl-clipboard` package)
- **X11 fallback**: Uses `xclip` when Wayland tools aren't available
- **Text clipboard**: Copy/paste text between Resonite and system clipboard
- **Image clipboard**: Copy/paste images (PNG format)
- **Configurable**: Mod configuration options via ResoniteModLoader
- **Error handling**: Robust error handling with timeouts for all clipboard operations
- **Discovery mode**: Optional debugging mode to scan for inspector/font candidates (disabled by default)

## Backends

| Backend | Tools Required | Status |
|---------|----------------|--------|
| Wayland | `wl-copy`, `wl-paste` (wl-clipboard package) | Primary |
| X11 | `xclip` | Fallback |

## Installation

### Prerequisites

Install one of these from your package manager:

- **Wayland sessions**: `wl-clipboard` package (provides `wl-copy` and `wl-paste`)
- **X11 sessions**: `xclip`

### For Steam Users

If you launch through Steam, you may need to copy the required binaries into your Resonite directory:

```bash
cp /usr/bin/wl-copy ~/.steam/steam/steamapps/common/Resonite
cp /usr/bin/wl-paste ~/.steam/steam/steamapps/common/Resonite
cp /usr/bin/xclip ~/.steam/steam/steamapps/common/Resonite
```

### Mod Installation

1. Build the mod: `dotnet build`
2. Copy the output DLL to your Resonite mods folder: `cp bin/Debug/net10.0/WaylandClipboard.dll ~/.steam/steam/steamapps/common/Resonite/rml_mods/`

## Configuration

The mod supports the following configuration options (via ResoniteModLoader's config system):

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `EnableDiscovery` | `bool` | `false` | Enable discovery mode for debugging (logs inspector/font candidates) |
| `ClipboardTimeoutMs` | `int` | `5000` | Timeout in milliseconds for clipboard operations (0 = no timeout) |

To configure, use ResoniteModLoader's configuration interface in-game.

## Building

### Prerequisites

- .NET 10.0 SDK
- Resonite installed (for ResoniteModLoader.dll, HarmonyLib.dll, etc.)

### Build Commands

```bash
# Build main mod
dotnet build

# Build and copy to Resonite mods folder
dotnet build && dotnet build -t:Copy
```

## Testing

The project includes comprehensive unit tests for core logic.

### Test Structure

```
ResoniteWaylandClipboard/
├── WaylandClipboard.Core/          # Pure logic (testable, no Resonite dependencies)
│   ├── DiscoveryTools.cs       # Keyword matching, type scoring
│   ├── BackendDetector.cs     # Backend detection logic
│   └── IProcessRunner.cs      # Interface for mocking Process.Start
├── WaylandClipboard.Tests/       # xUnit test project
│   ├── DiscoveryToolsTests.cs  # Tests for keyword matching, scoring
│   └── BackendDetectionTests.cs # Tests for backend detection
└── WaylandClipboard.csproj      # Main mod (requires ResoniteModLoader)
```

### Running Tests

```bash
# Run all tests
dotnet test WaylandClipboard.Tests/WaylandClipboard.Tests.csproj

# Run specific test category
dotnet test WaylandClipboard.Tests/WaylandClipboard.Tests.csproj --filter "FullyQualifiedName~DiscoveryTools"
```

**Current test status**: 24 tests passing (18 DiscoveryTools + 3 BackendDetection + 3 placeholder)

Note: Some tests (Configuration, InputValidation, MimeType) require ResoniteModLoader.dll and cannot be run without the Resonite environment.

## Hot Reload (Development)

Hot reload support is optional and only active when `ResoniteHotReloadLib.dll` is present.

To enable hot reload code paths while developing, build with:

```bash
dotnet build -p:DefineConstants="DEBUG;RML_HOTRELOAD"
```

## Project Structure

```
ResoniteWaylandClipboard/
├── WaylandClipboard.Core/          # Core library (no Resonite dependencies)
│   ├── DiscoveryTools.cs
│   ├── BackendDetector.cs
│   └── IProcessRunner.cs
├── WaylandClipboard.Tests/       # Test project
│   ├── DiscoveryToolsTests.cs
│   ├── BackendDetectionTests.cs
│   ├── ConfigurationTests.cs    # (placeholder - needs ResoniteModLoader)
│   ├── MimeTypeTests.cs        # (placeholder - needs refactoring)
│   └── InputValidationTests.cs  # (placeholder - needs ResoniteModLoader)
├── WaylandClipboard.cs           # Main mod class
├── WaylandClipboard.Core.csproj    # Core project file
├── WaylandClipboard.Tests.csproj  # Test project file
├── WaylandClipboard.csproj         # Main project file
├── README.md
└── UNLICENSE
```

## Improvements Made

- ✅ Made discovery mode configurable (default: off) via `EnableDiscovery` config
- ✅ Added clipboard backend validation (check if tools exist before using)
- ✅ Added comprehensive error handling (try-catch) to all clipboard operations
- ✅ Added null checks for `Process.Start` returns
- ✅ Fixed logging misuse (separated Info/Warn/Error levels)
- ✅ Added configurable timeouts via `ClipboardTimeoutMs` config
- ✅ Fixed MIME type handling (UTF8_STRING mapping only for X11)
- ✅ Added input validation (null handling for SetText/SetBitmap)
- ✅ Created WaylandClipboard.Core for testable logic
- ✅ Added 24 unit tests (xUnit + NSubstitute)

## License

This project is released under the UNLICENSE (public domain).
