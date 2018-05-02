using Dash.Engine.Graphics.OpenGL;

/* TextureParams.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class TextureParamPack
    {
        public TextureMagFilter MagFilter;
        public TextureMinFilter MinFilter;
        public TextureWrapMode WrapS;
        public TextureWrapMode WrapT;

        public TextureParamPack()
        {
            MagFilter = TextureMagFilter.Nearest;
            MinFilter = TextureMinFilter.Nearest;
            WrapS = TextureWrapMode.ClampToEdge;
            WrapT = TextureWrapMode.ClampToEdge;
        }

        public TextureParamPack(TextureMagFilter magFilter, TextureMinFilter minFilter, TextureWrapMode wrapMode)
        {
            MagFilter = magFilter;
            MinFilter = minFilter;
            WrapS = WrapT = wrapMode;
        }

        public TextureParamPack(TextureMagFilter magFilter, TextureMinFilter minFilter, 
            TextureWrapMode wrapS, TextureWrapMode wrapT)
        {
            MagFilter = magFilter;
            MinFilter = minFilter;
            WrapS = wrapS;
            WrapT = wrapT;
        }
    }
}
