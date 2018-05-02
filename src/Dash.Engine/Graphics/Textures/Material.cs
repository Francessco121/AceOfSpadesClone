/* Material.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class Material : Texture
    {
        public float ShineDamper = 1;
        public float Reflectivity = 0;

        public bool HasTransparency = false;

        public Material() { }
        public Material(TextureParamPack paramPack)
            : base(paramPack) { }
    }
}
