using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Elements.Assets {
    // Dummy AlphaHandling enum - real one is in Elements.Core
    public enum AlphaHandling {
        KeepOriginal = 0,
    }
    
    // Bitmap2D is defined in Elements.Assets.dll in the real game
    public class Bitmap2D {
        public void Save(Stream stream, string format) { }
        // Real signature: Load(Stream stream, string extension, bool? generateMipmaps, AlphaHandling alphaHandling = AlphaHandling.KeepOriginal, int maxSize = int.MaxValue, float sizeRatio = 1f)
        public static Bitmap2D Load(Stream stream, string extension, bool? generateMipmaps, AlphaHandling alphaHandling = AlphaHandling.KeepOriginal, int maxSize = int.MaxValue, float sizeRatio = 1f) => new Bitmap2D();
        public static Bitmap2D Load(string path) => new Bitmap2D();
    }
}
