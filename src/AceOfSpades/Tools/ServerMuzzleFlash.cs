using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AceOfSpades.Tools
{
    public class ServerMuzzleFlash : IMuzzleFlash
    {
        public bool Visible { get; private set; }

        public void Show()
        {
            Visible = true;
        }

        public void Hide()
        {
            Visible = false;
        }

        public void Dispose() { }
    }
}
