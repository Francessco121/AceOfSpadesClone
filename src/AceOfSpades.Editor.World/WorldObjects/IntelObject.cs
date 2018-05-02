using AceOfSpades.Graphics;
using AceOfSpades.IO;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using Dash.Engine.Graphics.OpenGL;

namespace AceOfSpades.Editor.World.WorldObjects
{
    public class IntelObject : WorldObject
    {
        public Team Team
        {
            get { return team; }
            set
            {
                team = value;
                if (value == Team.A)
                {
                    SetVoxelObject(redIntel);
                    Icon.Image.Color = Color.Red;
                }
                else
                {
                    SetVoxelObject(blueIntel);
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

        static VoxelObject redIntel;
        static VoxelObject blueIntel;
        static Texture intelTex;

        public IntelObject(Vector3 position) 
            : base(position)
        {
            if (redIntel == null)
            {
                redIntel = AssetManager.LoadVoxelObject("Models/intel-red.aosm", BufferUsageHint.StaticDraw);
                blueIntel = AssetManager.LoadVoxelObject("Models/intel-blue.aosm", BufferUsageHint.StaticDraw);
                intelTex = GLoader.LoadTexture("Textures/Gui/intel.png");
            }

            Icon.Size = new Vector2(16, 16);
            Icon.Offset = redIntel.UnitSize / 2f;
            Icon.Image = new Image(intelTex);

            Team = Team.A;
            EditorName = "Intel";
        }

        public override WorldObjectDescription CreateIODescription()
        {
            WorldObjectDescription desc = base.CreateIODescription();
            desc.Tag = "Intel";
            desc.AddField("Team", (byte)Team);
            return desc;
        }
    }
}
