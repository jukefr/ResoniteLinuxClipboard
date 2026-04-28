using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Renderite.Host {
    public interface IClipboardInterface {
        bool ContainsText { get; }
        bool ContainsFiles { get; }
        bool ContainsImage { get; }
        Task<List<string>> GetFiles();
        Task<object> GetImage();
        Task<string> GetText();
        Task<bool> SetBitmap(object bitmap);
        Task<bool> SetText(string text);
    }

    public class LinuxClipboardInterface : IClipboardInterface, IDisposable {
        public bool ContainsText => true;
        public bool ContainsFiles => false;
        public bool ContainsImage => false;
        public void Dispose() { }
        public Task<List<string>> GetFiles() => Task.FromResult(new List<string>());
        public Task<object> GetImage() => Task.FromResult<object>(null);
        public Task<string> GetText() => Task.FromResult("");
        public Task<bool> SetBitmap(object bitmap) => Task.FromResult(true);
        public Task<bool> SetText(string text) => Task.FromResult(true);
    }
}
