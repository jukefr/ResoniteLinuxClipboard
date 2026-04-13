# WaylandClipboard

a [resonitemodloader](https://github.com/resonite-modding-group/ResoniteModLoader) mod that uses `wl-clipboard` for Wayland clipboard support in Resonite.

## backends

- primary backend: Wayland clipboard tools (`wl-copy` + `wl-paste`)
- fallback backend: X11 clipboard tool (`xclip`)

install one of these from your package manager:

- Wayland sessions: `wl-clipboard` package (provides `wl-copy` and `wl-paste`)
- X11 sessions: `xclip`

if you launch through steam, you may need to copy the required binaries into your Resonite directory, for example:

- `cp /usr/bin/wl-copy ~/.steam/steam/steamapps/common/Resonite`
- `cp /usr/bin/wl-paste ~/.steam/steam/steamapps/common/Resonite`
- `cp /usr/bin/xclip ~/.steam/steam/steamapps/common/Resonite`

## hot reload (development)

hot reload support is optional and only active when `ResoniteHotReloadLib.dll` is present.

to enable hot reload code paths while developing, build with:

- `dotnet build -p:DefineConstants="DEBUG;RML_HOTRELOAD"`
