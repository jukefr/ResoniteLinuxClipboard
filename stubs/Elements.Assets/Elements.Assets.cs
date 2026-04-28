using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Elements.Assets {
    public class CommonClipboard {
        public struct ImageFormat {
            public string OLE { get; set; }
            public string Extension { get; set; }
            public string MimeType { get; set; }
        }

        public static ImageFormat[] ImageFormats { get; } = Array.Empty<ImageFormat>();
    }

    public class Bitmap2D {
        public void Save(Stream stream, string format) { }
        public static Bitmap2D Load(Stream stream, string extension, bool unknown) => new Bitmap2D();
        public static Bitmap2D Load(string path) => new Bitmap2D();
    }
}
