using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Elements.Assets;

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
        public void Dispose() { }
        public Task<List<string>> GetFiles() => Task.FromResult(new List<string>());
        public Task<Bitmap2D> GetImage() => Task.FromResult<Bitmap2D>(null);
        public Task<string> GetText() => Task.FromResult("");
        public Task<bool> SetBitmap(Bitmap2D bitmap) => Task.FromResult(true);
        public Task<bool> SetText(string text) => Task.FromResult(true);
    }
}
