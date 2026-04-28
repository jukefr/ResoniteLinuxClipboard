using System;
using System.Collections.Immutable;
using System.IO;

namespace Elements.Assets {
    public class CommonClipboard {
        public class ImageFormat {
            public ImageFormat OLE { get; } = new ImageFormat();
            public ImageFormat Extension { get; } = new ImageFormat();
            public string MimeType { get; } = "";
        }

        public ImmutableArray<ImageFormat> ImageFormats { get; }
    }

    public class Bitmap2D {
        public void Save(object streamOrPath, object format) { }
        public static Bitmap2D Load(Stream stream, string extension, bool unknown) => new Bitmap2D();
        public static Bitmap2D Load(string path) => new Bitmap2D();
    }
}
