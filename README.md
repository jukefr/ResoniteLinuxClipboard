# LinuxClipboard

A mod for Resonite that adds Linux clipboard support using [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).

**Repository**: [https://github.com/jukefr/ResoniteLinuxClipboard](https://github.com/jukefr/ResoniteLinuxClipboard)

## What it does

This mod lets you copy and paste between Resonite and your system clipboard on Linux. It handles both text and images (PNG).

The mod tries to use `wl-copy`/`wl-paste` (from `wl-clipboard`) on Wayland, and falls back to `xclip` on X11.

## Requirements

Install one of these from your package manager:

- **Wayland**: `wl-clipboard` (provides `wl-copy` and `wl-paste`)
- **X11**: `xclip`

If you're running Resonite through Steam, you might need to copy these binaries into your Resonite folder:

```bash
cp /usr/bin/wl-copy ~/.steam/steamapps/common/Resonite/
cp /usr/bin/wl-paste ~/.steam/steamapps/common/Resonite/
cp /usr/bin/xclip ~/.steam/steamapps/common/Resonite/
```

## Building and installing

```bash
dotnet build
```

Then copy the output to your Resonite mods folder:

```bash
cp bin/Debug/net10.0/LinuxClipboard.dll ~/.steam/steamapps/common/Resonite/rml_mods/
```

There's also a build target that copies it for you:

```bash
dotnet build -t:Copy
```

## Configuration

You can tweak the clipboard operation timeout (in milliseconds) through ResoniteModLoader's config system. The setting is called `ClipboardTimeoutMs` and defaults to 5000 (0 disables the timeout).

## Development stuff

The project is split into a core library (`LinuxClipboard.Core`) without Resonite dependencies, making it easier to test. Tests are in `LinuxClipboard.Tests` using xUnit.

If you're working on hot reload support, build with:

```bash
dotnet build -p:DefineConstants="DEBUG;RML_HOTRELOAD"
```

This only does something if `ResoniteHotReloadLib.dll` is present at runtime.

## Project layout

```
ResoniteLinuxClipboard/
├── LinuxClipboard.Core/          # Core logic, no Resonite dependencies
│   ├── BackendDetector.cs
│   └── IProcessRunner.cs
├── LinuxClipboard.Tests/        # xUnit tests
├── LinuxClipboard.cs            # Main mod class
└── [*.csproj files]
```

## License

GNU GPL-3.0 - see [LICENSE](LICENSE) for details.
