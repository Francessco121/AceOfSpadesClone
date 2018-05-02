using System;

namespace AceOfSpades.Tools
{
    public interface IMuzzleFlash : IDisposable
    {
        void Show();
        void Hide();
    }
}
