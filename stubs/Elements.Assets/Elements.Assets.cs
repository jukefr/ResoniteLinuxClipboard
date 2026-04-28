using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// In the real game, ImageFormat is in Renderite.Host namespace but inside Elements.Assets.dll
namespace Renderite.Host {
    public struct ImageFormat {
        public string OLE { get; set; }
        public string Extension { get; set; }
        public string MimeType { get; set; }
    }
    
    // CommonClipboard is also in Renderite.Host namespace (in Elements.Assets.dll)
    public class CommonClipboard {
        public static ImageFormat[] ImageFormats { get; } = Array.Empty<ImageFormat>();
    }
}

namespace Elements.Assets {
    // Bitmap2D is in Elements.Assets namespace (in Elements.Assets.dll)
    public class Bitmap2D {
        public void Save(Stream stream, string format) { }
        public static Bitmap2D Load(Stream stream, string extension, bool unknown) => new Bitmap2D();
        public static Bitmap2D Load(string path) => new Bitmap2D();
    }
}
