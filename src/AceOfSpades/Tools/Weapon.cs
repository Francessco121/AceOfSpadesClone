using AceOfSpades.Net;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;

/* Weapon.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public abstract class Weapon : Item
    {
        protected MasterRenderer renderer;

        public Weapon(MasterRenderer renderer, ItemManager itemManager, ItemType type)
            : base(itemManager, type | ItemType.Weapon)
        {
            this.renderer = renderer;
        }

        protected void LoadModel(string modelFilePath)
        {
            if (GlobalNetwork.IsServer)
                return;

            Renderer.VoxelObject = AssetManager.LoadVoxelObject(modelFilePath, BufferUsageHint.StaticDraw);
        }
    }
}
