using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;

namespace AceOfSpades.Characters
{
    public abstract class Character : GameObject
    {
        public VoxelObject VoxelObject
        {
            get { return Renderer.VoxelObject; }
        }

        public Vector3 Size
        {
            get { return CharacterController.Size; }
        }

        public CharacterController CharacterController { get; }
        protected VoxelRenderComponent Renderer { get; }

        public Character(Vector3 position, float height, float crouchHeight, float radius)
            : base(position)
        {
            AddComponent(CharacterController = new CharacterController(new Vector3(radius * 2, height, radius * 2), crouchHeight));

            if (GlobalNetwork.IsClient)
                AddComponent(Renderer = new VoxelRenderComponent());
        }
    }
}
