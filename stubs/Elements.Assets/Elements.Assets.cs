using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Elements.Assets {
    // Bitmap2D is defined in Elements.Assets.dll in the real game
    public class Bitmap2D {
        public void Save(Stream stream, string format) { }
        public static Bitmap2D Load(Stream stream, string extension, bool unknown) => new Bitmap2D();
        public static Bitmap2D Load(string path) => new Bitmap2D();
    }
}
