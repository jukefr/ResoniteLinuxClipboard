using System;

namespace Renderite.Host {
    public interface LinuxClipboardInterface {
        string GetText();
        void SetText(string text);
        object GetImage();
        void SetBitmap(object bitmap);
    }
}
