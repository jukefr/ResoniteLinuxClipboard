using System;
using System.IO;

namespace Elements.Assets {
    public class CommonClipboard {
        public struct ImageFormat {
            public string OLE { get; }
            public string Extension { get; }
            public string MimeType { get; }
        }

        public static ImageFormat[] ImageFormats { get; } = Array.Empty<ImageFormat>();
    }

    public class Bitmap2D {
        public void Save(Stream stream, string format) { }
        public static Bitmap2D Load(Stream stream, string extension, bool unknown) => new Bitmap2D();
        public static Bitmap2D Load(string path) => new Bitmap2D();
    }
}
