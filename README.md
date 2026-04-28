# LinuxClipboard

Adds Linux clipboard support to Resonite so you can copy and paste between Resonite and your system clipboard.

Supports both text and images (PNG). Works on Wayland and X11.

## Quick start

1. **Install system tools** — make sure you have `wl-clipboard` (Wayland) or `xclip` (X11) installed. See [Requirements](#requirements) below.
2. **Copy binaries** (if running through Steam) — copy the tool binaries into your Resonite folder. See [Requirements](#requirements) below.
3. **Install the mod** — drop `LinuxClipboard.dll` into `Resonite/rml_mods/`
4. **Done** — select text or images in Resonite and copy them. They'll appear in your system clipboard (and vice versa).

## Requirements

You need one of these installed on your system:

- **Wayland**: `wl-clipboard` (provides `wl-copy` and `wl-paste`)
- **X11**: `xclip`

If you're running Resonite through Steam, the binaries live in `/usr/bin/` but Resonite can't find them. Copy them into your Resonite folder:

```bash
cp /usr/bin/wl-copy ~/.steam/steamapps/common/Resonite/
cp /usr/bin/wl-paste ~/.steam/steamapps/common/Resonite/
cp /usr/bin/xclip ~/.steam/steamapps/common/Resonite/
```

## Configuration

You can change the clipboard operation timeout (in milliseconds) through ResoniteModLoader's Mod Settings. The setting is called `ClipboardTimeoutMs` and defaults to 5000. Set it to `0` to disable the timeout entirely.

## Building from source

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

For hot reload support, build with:

```bash
dotnet build -p:DefineConstants="DEBUG;RML_HOTRELOAD"
```

This only does something if `ResoniteHotReloadLib.dll` is present at runtime.

## License

GNU GPL-3.0 — see [LICENSE](LICENSE) for details.
