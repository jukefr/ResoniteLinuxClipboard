using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Elements.Assets {
    // ImageFormat is defined in Renderite.Host in the real game
    // We'll reference it once Renderite.Host.dll is built
    public class CommonClipboard {
        public static Renderite.Host.ImageFormat[] ImageFormats { get; } = Array.Empty<Renderite.Host.ImageFormat>();
    }
}
