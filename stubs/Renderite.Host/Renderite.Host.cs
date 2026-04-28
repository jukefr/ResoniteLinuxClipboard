using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// Reference Bitmap2D from Elements.Assets.dll
using Bitmap2D = Elements.Assets.Bitmap2D;

namespace Renderite.Host {
    // CommonClipboard with ImageFormat is defined in Renderite.Host.dll in the real game
    public static class CommonClipboard {
        public readonly struct ImageFormat {
            public string OLE { get; }
            public string Extension { get; }
            public string MimeType { get; }
            
            public ImageFormat(string ole, string extension) {
                OLE = ole;
                Extension = extension;
                MimeType = ole;
            }
        }
        
        // In real game this is a property with getter, not a field
        public static ImageFormat[] ImageFormats { get; } = new ImageFormat[0];
    }

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
        public System.Nullable<CommonClipboard.ImageFormat> GetImageMime() => null;
        
        public void Dispose() { }
        public Task<List<string>> GetFiles() => Task.FromResult(new List<string>());
        public Task<Bitmap2D> GetImage() => Task.FromResult<Bitmap2D>(null);
        public Task<string> GetText() => Task.FromResult("");
        public Task<bool> SetBitmap(Bitmap2D bitmap) => Task.FromResult(true);
        public Task<bool> SetText(string text) => Task.FromResult(true);
    }
}
