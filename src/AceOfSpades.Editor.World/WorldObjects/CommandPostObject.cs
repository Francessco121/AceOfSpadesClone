using AceOfSpades.Graphics;
using AceOfSpades.IO;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using Dash.Engine.Graphics.OpenGL;

namespace AceOfSpades.Editor.World.WorldObjects
{
    public class CommandPostObject : WorldObject
    {
        public Team Team
        {
            get { return team; }
            set
            {
                team = value;
                if (value == Team.A)
                {
                    SetVoxelObject(redCommandPost);
                    Icon.Image.Color = Color.Red;
                }
                else
                {
                    SetVoxelObject(blueCommandPost);
                    Icon.Image.Color = Color.Blue;
                }
            }
        }
        Team team;

        [EditableField("Is Team B")]
        bool isBlueTeam
        {
            get { return Team == Team.B; }
            set { Team = value ? Team.B : Team.A; }
        }

        static VoxelObject redCommandPost;
        static VoxelObject blueCommandPost;
        static Texture commandpostTex;

        public CommandPostObject(Vector3 position) 
            : base(position)
        {
            if (redCommandPost == null)
            {
                redCommandPost = AssetManager.LoadVoxelObject("Models/commandpost-red.aosm", BufferUsageHint.StaticDraw);
                blueCommandPost = AssetManager.LoadVoxelObject("Models/commandpost-blue.aosm", BufferUsageHint.StaticDraw);
                commandpostTex = GLoader.LoadTexture("Textures/Gui/commandpost.png");
            }

            Icon.Size = new Vector2(16, 16);
            Icon.Offset = redCommandPost.UnitSize / 2f;
            Icon.Image = new Image(commandpostTex);

            Team = Team.A;
            EditorName = "Command Post";
        }

        public override WorldObjectDescription CreateIODescription()
        {
            WorldObjectDescription desc = base.CreateIODescription();
            desc.Tag = "CommandPost";
            desc.AddField("Team", (byte)Team);
            return desc;
        }
    }
}
