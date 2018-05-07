using AceOfSpades.Net;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using System;

/* MPPlayer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Characters
{
    public abstract class MPPlayer : Player, INetCreatable
    {
        public NetCreatableInfo StateInfo { get; private set; }

        public override bool IsRenderingThirdperson
        {
            get { return StateInfo != null && !StateInfo.IsAppOwner; }

            // TODO: might be a better way to handle this....
            set { throw new InvalidOperationException("Cannot enter thirdperson in a multiplayer match!"); }
        }

        public MPPlayer(MasterRenderer renderer, World world, SimpleCamera camera, Vector3 position, Team team)
            : base(renderer, world, camera, position, team)
        {
            if (!DashCMD.IsCVarDefined("log_mpplayer"))
                DashCMD.SetCVar("log_mpplayer", false);
        }

        protected void CreateStarterBackpack()
        {
            ItemManager.SetItems(new Item[] {
                new Rifle(ItemManager, masterRenderer),
                new SMG(ItemManager, masterRenderer),
                new Shotgun(ItemManager, masterRenderer),
                new Grenade(ItemManager, masterRenderer),
                new Spade(ItemManager, masterRenderer),
                new BlockItem(ItemManager, masterRenderer),
                new MelonLauncher(ItemManager, masterRenderer)
            }, 0);
        }

        public virtual void OnNetworkInstantiated(NetCreatableInfo stateInfo)
        {
            StateInfo = stateInfo;
            if (DashCMD.GetCVar<bool>("log_mpplayer"))
                DashCMD.WriteStandard("[MPPlayer - {1}] Instantiated with id {0}", stateInfo.Id, 
                    stateInfo.IsAppOwner ? "Ours" : "Not Ours");
        }

        public virtual void OnNetworkDestroy()
        {
            if (DashCMD.GetCVar<bool>("log_mpplayer"))
                DashCMD.WriteStandard("[MPPlayer - {1}] Destroyed with id {0}", StateInfo.Id,
                    StateInfo.IsAppOwner ? "Ours" : "Not Ours");

            Dispose();
        }
    }
}
