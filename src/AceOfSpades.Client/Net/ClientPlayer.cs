using AceOfSpades.Characters;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Graphics;

namespace AceOfSpades.Client.Net
{
    public abstract class ClientPlayer : MPPlayer
    {
        public ClientPlayer(MasterRenderer renderer, World world, SimpleCamera camera, Vector3 position, Team team) 
            : base(renderer, world, camera, position, team)
        { }

        public abstract void OnClientInbound(PlayerSnapshot snapshot);
    }
}
