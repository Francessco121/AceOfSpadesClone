using AceOfSpades.Characters;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Physics;
using Dash.Net;
using System;
using System.Collections.Generic;

/* MPWorld.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public class MPWorld : ClientWorld
    {
        public override float TimeOfDay
        {
            get { return Renderer.Sky.currentHour; }
            set { Renderer.Sky.currentHour = value; }
        }

        public ClientMPPlayer OurPlayer { get; private set; }

        Dictionary<ushort, ClientPlayer> players;
        Dictionary<ushort, GameObject> physEntities;

        AOSClient client;
        SnapshotNetComponent snapshotComponent;
        ObjectNetComponent objectComponent;
        RemoteChannel channel;

        public MPWorld(MasterRenderer renderer) 
            : base(renderer)
        {
            players      = new Dictionary<ushort, ClientPlayer>();
            physEntities = new Dictionary<ushort, GameObject>();
            TimeOfDay = 10;

            // Grab network components and the world channel
            client            = AOSClient.Instance;
            snapshotComponent = client.GetComponent<SnapshotNetComponent>();
            objectComponent   = client.GetComponent<ObjectNetComponent>();
            channel           = client.GetChannel(AOSChannelType.World);

            // Add remotes
            channel.AddRemoteEvent("Client_ThrowGrenade", R_ThrowGrenade);
            channel.AddRemoteEvent("Client_ShootMelon", R_ShootMelon);
            channel.AddRemoteEvent("Client_ServerImpact", R_ServerImpact);
            channel.AddRemoteEvent("Client_RolledBackServerPlayer", R_RolledBackServerPlayer);

            // Hook into component events
            objectComponent.OnCreatableInstantiated  += ObjectComponent_OnCreatableInstantiated;
            objectComponent.OnCreatableDestroyed     += ObjectComponent_OnCreatableDestroyed;
            snapshotComponent.OnWorldSnapshotInbound += SnapshotComponent_OnWorldSnapshotInbound;
        }

        public override void Dispose()
        {
            // Dipose of each player (correctly removes and lights associated with them as well)
            foreach (Player player in players.Values)
                player.Dispose();

            // Remove instantiation events
            objectComponent.RemoveInstantiationEvent("Client_CreatePlayer");

            // Remove remotes
            channel.RemoveRemoteEvent("Client_ThrowGrenade");
            channel.RemoveRemoteEvent("Client_ShootMelon");
            channel.RemoveRemoteEvent("Client_ServerImpact");
            channel.RemoveRemoteEvent("Client_RolledBackServerPlayer");

            // Unhook component events
            objectComponent.OnCreatableInstantiated  -= ObjectComponent_OnCreatableInstantiated;
            objectComponent.OnCreatableDestroyed     -= ObjectComponent_OnCreatableDestroyed;
            snapshotComponent.OnWorldSnapshotInbound -= SnapshotComponent_OnWorldSnapshotInbound;

            base.Dispose();
        }

        void SnapshotComponent_OnWorldSnapshotInbound(object sender, WorldSnapshot e)
        {
            // Synchronize the time of day
            TimeOfDay = e.Time;
        }

        private void ObjectComponent_OnCreatableInstantiated(object sender, NetCreatableInfo creatable)
        {
            ClientPlayer player = creatable.Creatable as ClientPlayer;
            if (player != null)
            {
                players.Add(creatable.Id, player);
                if (creatable.IsAppOwner)
                    OurPlayer = (ClientMPPlayer)player;

                AddGameObject(player);
            }
            else
            {
                GameObject gameObject = creatable.Creatable as GameObject;
                AddGameObject(gameObject);
                if (gameObject.HasComponent<PhysicsBodyComponent>())
                    physEntities.Add(creatable.Id, gameObject);
            }
        }

        void ObjectComponent_OnCreatableDestroyed(object sender, NetCreatableInfo creatable)
        {
            ClientPlayer player = creatable.Creatable as ClientPlayer;
            if (player != null)
            {
                // If a player was destroyed we need to remove 
                // anything associated with them.
                players.Remove(creatable.Id);
                player.Dispose();

                if (player == OurPlayer)
                {
                    // Notify the player
                    OurPlayer.OnKilled();

                    // Remove the player
                    OurPlayer = null;
                }
            }
            else
            {
                GameObject gameObject = creatable.Creatable as GameObject;
                if (gameObject.HasComponent<PhysicsBodyComponent>())
                    physEntities.Remove(creatable.Id);

                gameObject.Dispose();
            }
        }

        public override void OnScreenResized(int width, int height)
        {
            base.OnScreenResized(width, height);
        }

        void R_RolledBackServerPlayer(NetConnection server, NetBuffer data, ushort numArgs)
        {
            Vector3 origin = new Vector3(data.ReadFloat(), data.ReadFloat(), data.ReadFloat());
            Team team = (Team)data.ReadByte();
            debugRenderer.AddPlayerRollback(origin, team == Team.A ? TeamAColor : TeamBColor);
        }

        void R_ServerImpact(NetConnection server, NetBuffer data, ushort numArgs)
        {
            Vector3 origin = new Vector3(data.ReadFloat(), data.ReadFloat(), data.ReadFloat());
            debugRenderer.AddBulletImpact(origin, Color.Blue);
        }

        void R_ThrowGrenade(NetConnection server, NetBuffer data, ushort numArgs)
        {
            float x = data.ReadFloat();
            float y = data.ReadFloat();
            float z = data.ReadFloat();

            float vx = data.ReadFloat();
            float vy = data.ReadFloat();
            float vz = data.ReadFloat();

            float power = data.ReadFloat();

            ThrowGrenadeRep(null, new Vector3(x, y, z), new Vector3(vx, vy, vz), power);
        }

        void R_ShootMelon(NetConnection server, NetBuffer data, ushort numArgs)
        {
            float x = data.ReadFloat();
            float y = data.ReadFloat();
            float z = data.ReadFloat();

            float vx = data.ReadFloat();
            float vy = data.ReadFloat();
            float vz = data.ReadFloat();

            ShootMelonRep(null, new Vector3(x, y, z), new Vector3(vx, vy, vz));
        }

        public override void GunFired(float verticalRecoil, float horizontalRecoil, float kickback)
        {
            OurPlayer.ApplyRecoil(verticalRecoil, horizontalRecoil, kickback);
            base.GunFired(verticalRecoil, horizontalRecoil, kickback);
        }

        public override PlayerRaycastResult RaycastPlayers(Ray ray, float maxDist = float.MaxValue, params Player[] ignore)
        {
            ClientPlayer hitPlayer = null;
            float? hitPlayerAt = null;

            foreach (ClientPlayer otherPlayer in players.Values)
            {
                // Make sure we aren't ignoring this player
                if (ignore.Length == 0 || !Array.Exists(ignore, (x => x == otherPlayer)))
                {
                    Ray newRay = new Ray(ray.Origin - otherPlayer.Transform.Position, ray.Direction);

                    float? dist;
                    // Check for intersection
                    if (newRay.Intersects(otherPlayer.GetOrientatedBoundingBox(otherPlayer.GetCamera().Yaw), out dist))
                    {
                        // If the distance is out of bounds, ignore
                        if (dist.Value > maxDist)
                            continue;

                        // Only update the intersected player if it was closer than the last
                        if (!hitPlayerAt.HasValue || dist.Value < hitPlayerAt.Value)
                        {
                            hitPlayer = otherPlayer;
                            hitPlayerAt = dist.Value;
                        }
                    }
                }
            }

            if (hitPlayer != null)
                return new PlayerRaycastResult(ray, true, ray.Origin + ray.Direction * hitPlayerAt.Value, hitPlayerAt, hitPlayer);
            else
                return new PlayerRaycastResult(ray);
        }

        public override void FireBullet(Player player, Vector3 origin, Vector3 dir, Vector3 recoil,
            int damage, float playerDamage, float maxDist = float.MaxValue)
        {
            // Signal server that we fired a bullet at this time
            OurPlayer.ClientSnapshot.BulletSnapshot.EnqueueBullet(new NetworkBullet(origin, Camera.Active.Yaw, Camera.Active.Pitch));

            // Simulate bullet locally if enabled
            if (DashCMD.GetCVar<bool>("cl_impacts"))
            {
                Ray ray = new Ray(origin, dir + recoil);
                TerrainRaycastResult result = TerrainPhysics.Raycast(ray, true, maxDist);

                ClientPlayer hitPlayer = null;
                float? hitPlayerAt = null;
                foreach (ClientPlayer otherPlayer in players.Values)
                    if (otherPlayer != player)
                    {
                        if (DashCMD.GetCVar<bool>("cl_impacts"))
                            debugRenderer.AddPlayerRollback(otherPlayer.Transform.Position, Color.Green);

                        Ray newRay = new Ray(origin - otherPlayer.Transform.Position, dir);

                        float? dist;
                        if (newRay.Intersects(otherPlayer.GetOrientatedBoundingBox(), out dist))
                        {
                            if (!hitPlayerAt.HasValue || dist.Value < hitPlayerAt.Value)
                            {
                                hitPlayer = otherPlayer;
                                hitPlayerAt = dist.Value;
                            }
                        }
                    }

                if (result.Intersects && (!hitPlayerAt.HasValue || (hitPlayerAt.Value > result.IntersectionDistance.Value)))
                    debugRenderer.AddBulletImpact(ray.Origin + ray.Direction * result.IntersectionDistance.Value, Color.Red);
                else if (hitPlayerAt.HasValue)
                    debugRenderer.AddBulletImpact(ray.Origin + ray.Direction * hitPlayerAt.Value, Color.Red);
            }
        }

        public override Block SetBlock(IndexPosition chunkIndex, IndexPosition blockPos, Block block, bool placement)
        {
            channel.FireEvent("Server_SetBlock", client.ServerConnection,
                (short)chunkIndex.X, (short)chunkIndex.Y, (short)chunkIndex.Z,
                (ushort)blockPos.X, (ushort)blockPos.Y, (ushort)blockPos.Z,
                block.R, block.G, block.B, block.Data.Value,
                placement);

            return block;
        }

        public override void Explode(Explosion explosion)
        {
            if (OurPlayer != null)
            {
                float distToCam = (explosion.Origin - Camera.Active.Position).Length;
                float factor = 5f / (distToCam * 0.3f); // maxShake / (distToCam * falloff)
                if (factor > 0.15f) // factor > minShake
                    OurPlayer.ShakeCamera(0.5f, factor);
            }

            float radius = explosion.PlayerRadius;

            for (int i = 0; i < grenades.Count; i++)
            {
                GrenadeEntity grenade = grenades[i];

                Vector3 dirTo = grenade.Transform.Position - explosion.Origin;
                float dist = dirTo.Length;
                if (dist > radius)
                    continue;

                dirTo += new Vector3(0, 1, 0);
                dirTo = dirTo.Normalize();

                float kickBack = 60 * (1f - (dist / radius));
                grenade.PhysicsBody.Velocity += dirTo * kickBack;
                grenade.Transform.Position.Y += 0.01f;
            }
        }

        public override void ThrowGrenade(Player owner, Vector3 origin, Vector3 dir, float power)
        {
            channel.FireEvent("Server_ThrowGrenade", client.ServerConnection,
                origin.X, origin.Y, origin.Z, 
                dir.X, dir.Y, dir.Z,
                power);
        }

        public override void ShootMelon(Player owner, Vector3 origin, Vector3 dir)
        {
            channel.FireEvent("Server_ShootMelon", client.ServerConnection,
                origin.X, origin.Y, origin.Z,
                dir.X, dir.Y, dir.Z);
        }

        public void ThrowGrenadeRep(Player owner, Vector3 origin, Vector3 dir, float power)
        {
            base.ThrowGrenade(owner, origin, dir, power);
        }

        public void ShootMelonRep(Player owner, Vector3 origin, Vector3 dir)
        {
            base.ShootMelon(owner, origin, dir);
        }

        public bool TryGetPlayer(ushort id, out ClientPlayer player)
        {
            return players.TryGetValue(id, out player);
        }

        public void LoadServerTerrain(NetBuffer data)
        {
            SetTerrain(new FixedTerrain(Renderer));

            DashCMD.WriteStandard("[MPWorld] Loading server world...");

            ushort numChunks = data.ReadUInt16();
            Chunk currentChunk = null;
            int blockI = 0;
            int ci = 0;

            while (ci <= numChunks && data.Position < data.Data.Length)
            {
                byte type = data.ReadByte();

                if (type == 0) // New Chunk
                {
                    int ix = data.ReadInt16();
                    int iy = data.ReadInt16();
                    int iz = data.ReadInt16();

                    if (currentChunk != null)
                        currentChunk.BakeColors();

                    IndexPosition ipos = new IndexPosition(ix, iy, iz);
                    currentChunk = new Chunk(Terrain, ipos, AceOfSpades.Terrain.ChunkToWorldCoords(ipos));
                    currentChunk.InitBlocks(Chunk.HSIZE, Chunk.VSIZE, Chunk.HSIZE);
                    currentChunk.State = ChunkState.Unbuilt;
                    currentChunk.IsDirty = true;
                    Terrain.Chunks.TryAdd(ipos, currentChunk);

                    blockI = 0;
                    ci++;
                }
                else if (type == 1) // Block section
                {
                    ushort numBlocks = data.ReadUInt16();
                    byte d = data.ReadByte();
                    Nybble2 n = new Nybble2(d);
                    byte r, g, b;
                    byte mat = n.Lower;

                    if (mat == Block.CUSTOM.Material)
                    {
                        r = data.ReadByte();
                        g = data.ReadByte();
                        b = data.ReadByte();
                    }
                    else
                    {
                        if (mat == Block.GRASS.Material)
                        {
                            r = Block.GRASS.R;
                            g = Block.GRASS.G;
                            b = Block.GRASS.B;
                        }
                        else
                        {
                            r = Block.STONE.R;
                            g = Block.STONE.G;
                            b = Block.STONE.B;
                        }
                    }

                    Block block = new Block(n, r, g, b);

                    for (int i = 0; i < numBlocks; i++)
                    {
                        int z = blockI % Chunk.HSIZE;
                        int y = (blockI / Chunk.HSIZE) % Chunk.VSIZE;
                        int x = blockI / (Chunk.VSIZE * Chunk.HSIZE);

                        currentChunk.Blocks[z, y, x] = block;
                        blockI++;
                    }
                }
            }

            if (currentChunk != null)
                currentChunk.BakeColors();

            Terrain.CreatedFromFile();
        }
    }
}
