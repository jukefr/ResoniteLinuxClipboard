using System;
using System.IO;

namespace Elements.Assets {
    public class CommonClipboard {
        public enum ImageFormat {
            OLE,
            Extension
        }

        public ImageFormat ImageFormats { get; }
        public string MimeType { get; }
    }

    public class Bitmap2D {
        public void Save(string path, object format) { }
        public void Save(string path, CommonClipboard.ImageFormat format) { }
        public static Bitmap2D Load(string path) => new Bitmap2D();
        public static Bitmap2D Load(MemoryStream stream, string extension, bool unknown) => new Bitmap2D();
    }
}
