using System;
using AceOfSpades.Characters;
using AceOfSpades.Client.Gui;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using AceOfSpades.IO;

/* SPWorld.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client
{
    public class SPWorld : ClientWorld
    {
        public override float TimeOfDay
        {
            get { return Renderer.Sky.currentHour; }
            set { Renderer.Sky.currentHour = value; }
        }

        public SPPlayer Player { get; }
        HUD hud;

        public SPWorld(MasterRenderer renderer)
            : base(renderer)
        {
            WorldDescription desc = LoadFromFile("default");

            Vector3 spawnPos = new Vector3(100, 400, 100);

            var commandposts = desc.GetObjectsByTag("CommandPost");
            foreach (WorldObjectDescription ob in commandposts)
            {
                Vector3 position = ob.GetVector3("Position");
                Team team = (Team)(ob.GetField<byte>("Team") ?? 0);
                CommandPost commandPost = new CommandPost(position, team);
                AddGameObject(commandPost);

                if (team == Team.A)
                    spawnPos = position;
            }

            var intels = desc.GetObjectsByTag("Intel");
            foreach (WorldObjectDescription ob in intels)
            {
                Vector3 position = ob.GetVector3("Position");
                Team team = (Team)(ob.GetField<byte>("Team") ?? 0);
                Intel editorIntel = new Intel(position, team);
                AddGameObject(editorIntel);
            }

            Player = new SPPlayer(Renderer, this, Camera.Active, spawnPos, Team.A);
            AddGameObject(Player);
            hud = new HUD(Renderer);
            hud.Player = Player;
            hud.ShowCharacterInformation = true;
            hud.Enable();

            Player.AttachCamera();
        }

        public override PlayerRaycastResult RaycastPlayers(Ray ray, float maxDist = float.MaxValue, params Player[] ignore)
        {
            return new PlayerRaycastResult(ray);
        }

        public override void GunFired(float verticalRecoil, float horizontalRecoil, float kickback)
        {
            Player.ApplyRecoil(verticalRecoil, horizontalRecoil, kickback);
            base.GunFired(verticalRecoil, horizontalRecoil, kickback);
        }

        public override void Explode(Explosion explosion)
        {
            base.Explode(explosion);

            float distToCam = (explosion.Origin - Camera.Active.Position).Length;
            float factor = 5f / (distToCam * 0.3f); // maxShake / (distToCam * falloff)
            if (factor > 0.15f) // factor > minShake
                Player.ShakeCamera(0.5f, factor);

            PlayerRaycastResult eResult = RaycastPlayer(explosion.Origin, Player, explosion.PlayerRadius);
            if (eResult.Intersects)
            {
                float damage = explosion.Damage / (eResult.IntersectionDistance.Value * explosion.DamageFalloff);
                Player.Damage(damage, "Explosion");
            }
        }

        public override void OnScreenResized(int width, int height)
        {
            base.OnScreenResized(width, height);
        }

        public override void Update(float deltaTime)
        {
            if (Player != null)
            {
                Player.CharacterController.IsStatic = !Terrain.Ready;
                hud.Update(deltaTime);
            }

            base.Update(deltaTime);
        }

        public override void Draw()
        {
            hud.Draw(Renderer.Sprites.SpriteBatch);

            base.Draw();
        }

        public override void Dispose()
        {
            hud.Disable();
            base.Dispose();
        }
    }
}
