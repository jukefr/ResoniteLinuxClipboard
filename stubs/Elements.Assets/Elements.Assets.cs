using System;
using System.IO;

namespace Elements.Assets {
    public class CommonClipboard {
        public class ImageFormat {
            public ImageFormat OLE { get; }
            public ImageFormat Extension { get; }
            public string MimeType { get; }
        }

        public ImageFormat ImageFormats { get; }
    }

    public class Bitmap2D {
        public void Save(Stream stream, object format) { }
        public void Save(Stream stream, string format) { }
        public static Bitmap2D Load(Stream stream, string extension, bool unknown) => new Bitmap2D();
        public static Bitmap2D Load(string path) => new Bitmap2D();
    }
}
