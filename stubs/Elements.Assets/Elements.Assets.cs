using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// In the real game:
// - Elements.Assets.dll contains:
//   - namespace Elements.Assets { class Bitmap2D }
//   - namespace Elements.Assets { class CommonClipboard { nested ImageFormat struct } }

namespace Elements.Assets {
    // Bitmap2D is in Elements.Assets namespace
    public class Bitmap2D {
        public void Save(Stream stream, string format) { }
        public static Bitmap2D Load(Stream stream, string extension, bool unknown) => new Bitmap2D();
        public static Bitmap2D Load(string path) => new Bitmap2D();
    }
    
    // CommonClipboard is in Elements.Assets namespace
    public class CommonClipboard {
        // ImageFormat is a nested struct inside CommonClipboard
        public struct ImageFormat {
            public string OLE { get; set; }
            public string Extension { get; set; }
            public string MimeType { get; set; }
        }
        
        public static ImageFormat[] ImageFormats { get; } = Array.Empty<ImageFormat>();
    }
}
