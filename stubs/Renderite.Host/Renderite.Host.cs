using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// ImageFormat and Bitmap2D are defined in Elements.Assets.dll (in Renderite.Host namespace)
// We reference them from the Elements.Assets assembly

namespace Renderite.Host {
    public interface IClipboardInterface {
        bool ContainsText { get; }
        bool ContainsFiles { get; }
        bool ContainsImage { get; }
        Task<List<string>> GetFiles();
        Task<Bitmap2D> GetImage();
        Task<string> GetText();
        Task<bool> SetBitmap(Bitmap2D bitmap);
        Task<bool> SetText(string text);
    }

    public class LinuxClipboardInterface : IClipboardInterface, IDisposable {
        public bool ContainsText => true;
        public bool ContainsFiles => false;
        public bool ContainsImage => false;
        
        // This method exists in the real game and returns Nullable<ImageFormat>
        public Nullable<ImageFormat> GetImageMime() => null;
        
        public void Dispose() { }
        public Task<List<string>> GetFiles() => Task.FromResult(new List<string>());
        public Task<Bitmap2D> GetImage() => Task.FromResult<Bitmap2D>(null);
        public Task<string> GetText() => Task.FromResult("");
        public Task<bool> SetBitmap(Bitmap2D bitmap) => Task.FromResult(true);
        public Task<bool> SetText(string text) => Task.FromResult(true);
    }
}
