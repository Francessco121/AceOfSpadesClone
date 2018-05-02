using System;
using Dash.Engine.Graphics.OpenGL;

/* CubeMap.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class CubeMap : IDisposable
    {
        public uint Id { get; private set; }
        public bool Bound { get; private set; }

        public CubeMap()
        {
            Id = GManager.GenTexture();
        }

        public void Bind()
        {
            Bound = true;
            GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        }

        public void Unbind()
        {
            Bound = false;
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

        void CheckBind()
        {
            if (!Bound)
                throw new InvalidOperationException("Cannot modify cube map, it is not bound!");
        }

        public void SetMinMag(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            CheckBind();

            // Setup texture paramters
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)magFilter);
        }

        public void SetWrap(TextureWrapMode sWrap, TextureWrapMode tWrap)
        {
            CheckBind();

            // Set the wrapping
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)sWrap);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)tWrap);
        }

        public void Dispose()
        {
            GL.DeleteTexture(Id);
        }
    }
}
