using AceOfSpades.Characters;
using AceOfSpades.IO;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.IO;
using Dash.Engine.Physics;
using Dash.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

/* ServerWorld.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public delegate void PlayerKilledHandler(PlayerDamage damageEvent);

    public class ServerWorld : World, IDisposable
    {
        public override float TimeOfDay
        {
            get { return timeOfDay; }
            set { timeOfDay = value; }
        }
        float timeOfDay = 9;

        public string CurrentWorldName { get; private set; }

        public event PlayerKilledHandler OnPlayerKilled;

        public WorldDescription Description { get; private set; }

        AOSServer server;
        SnapshotNetComponent snapshotComponent;
        ObjectNetComponent objectComponent;
        RemoteChannel channel;

        // This is concurrent, so that if the controlling screen
        // removes a player say, because they got killed, when
        // updating each player we won't have a collection modified exception.
        ConcurrentDictionary<NetConnection, ServerMPPlayer> players;

        Dictionary<ushort, GameObject> physEntities;

        public ServerWorld()
        {
            players      = new ConcurrentDictionary<NetConnection, ServerMPPlayer>();
            physEntities = new Dictionary<ushort, GameObject>();

            server            = AOSServer.Instance;
            snapshotComponent = server.GetComponent<SnapshotNetComponent>();
            objectComponent   = server.GetComponent<ObjectNetComponent>();
            channel           = server.GetChannel(AOSChannelType.World);

            channel.AddRemoteEvent("Server_SetBlock", R_SetBlock);
            channel.AddRemoteEvent("Server_ThrowGrenade", R_ThrowGrenade);

            objectComponent.OnCreatableInstantiated   += ObjectComponent_OnCreatableInstantiated;
            objectComponent.OnCreatableDestroyed      += ObjectComponent_OnCreatableDestroyed;
            snapshotComponent.OnWorldSnapshotOutbound += Server_OnWorldSnapshotOutbound;

            InitializeCMD();

            ConfigSection gameSection = Program.Config.GetSection("Game");

            if (gameSection == null)
                DashCMD.WriteError("[server.cfg - ServerWorld] Section 'Game' is missing!");
            else
            {
                string worldFile = gameSection.GetString("world-file");

                if (!string.IsNullOrWhiteSpace(worldFile))
                    LoadFromFile(worldFile);
                else
                    DashCMD.WriteError("[server.cfg - ServerWorld] Game.world-file is missing!");
            }
        }

        public override void Dispose()
        {
            channel.RemoveRemoteEvent("Server_SetBlock");
            channel.RemoveRemoteEvent("Server_ThrowGrenade");

            objectComponent.OnCreatableInstantiated   -= ObjectComponent_OnCreatableInstantiated;
            objectComponent.OnCreatableDestroyed      -= ObjectComponent_OnCreatableDestroyed;
            snapshotComponent.OnWorldSnapshotOutbound -= Server_OnWorldSnapshotOutbound;
            base.Dispose();
        }

        void Server_OnWorldSnapshotOutbound(object sender, WorldSnapshot e)
        {
            e.Time = timeOfDay;
        }

        private void ObjectComponent_OnCreatableInstantiated(object sender, NetCreatableInfo creatable)
        {
            ServerMPPlayer player = creatable.Creatable as ServerMPPlayer;
            if (player != null)
            {
                players.TryAdd(creatable.Owner, player);
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
            ServerMPPlayer player = creatable.Creatable as ServerMPPlayer;
            if (player != null)
            {
                // If a player was destroyed we need to remove 
                // anything associated with them.
                player.Dispose();

                players.TryRemove(creatable.Owner, out player);
            }
            else
            {
                GameObject gameObject = creatable.Creatable as GameObject;
                if (gameObject.HasComponent<PhysicsBodyComponent>())
                    physEntities.Remove(creatable.Id);

                gameObject.Dispose();
            }
        }

        void InitializeCMD()
        {
            if (DashCMD.IsCommandDefined("time"))
                return;

            DashCMD.SetCVar<float>("time_autoshift", 0);

            //DashCMD.AddCommand("saveworld", "Saves the current world to file", "saveworld <filename>",
            //    (args) =>
            //    {
            //        if (args.Length != 1)
            //            DashCMD.ShowSyntax("saveworld");
            //        else
            //        {
            //            string fileName = args[0];
            //            WorldIO.Save(fileName, new WorldDescription(Terrain));
            //            DashCMD.WriteImportant("Saved world: {0}.aosw", fileName);
            //        }
            //    });

            DashCMD.AddCommand("time", "Changes the time of day", "time [0-24]",
                (args) =>
                {
                    if (args.Length == 0)
                        DashCMD.WriteLine("Current Time: {0}", timeOfDay);
                    else
                    {
                        try
                        {
                            float newTime = float.Parse(args[0]);
                            newTime = MathHelper.Clamp(newTime, 0, 24);

                            timeOfDay = newTime;
                        }
                        catch (Exception)
                        {
                            DashCMD.WriteError("Invalid time.");
                        }
                    }
                });
        }

        public bool LoadFromFile(string fileName)
        {
            DashCMD.WriteImportant("[ServerWorld] Loading world '{0}'...", fileName);
            try
            {
                Description = WorldIO.Load(CurrentWorldName = fileName);
                SetTerrain(Description.Terrain);
                Terrain.LockBottomLayer = true;
                DashCMD.WriteImportant("[ServerWorld] Successfully loaded world '{0}'.", fileName);
                return true;
            }
            catch (IOException ioex)
            {
                DashCMD.WriteError("[ServerWorld] Failed to load world '{0}'!", fileName);
                DashCMD.WriteError(ioex);
                return false;
            }
        }

        void R_ThrowGrenade(NetConnection client, NetBuffer buffer, ushort numArgs)
        {
            float ox = buffer.ReadFloat();
            float oy = buffer.ReadFloat();
            float oz = buffer.ReadFloat();

            float dx = buffer.ReadFloat();
            float dy = buffer.ReadFloat();
            float dz = buffer.ReadFloat();

            float power = buffer.ReadFloat();

            ServerMPPlayer player;
            if (players.TryGetValue(client, out player))
            {
                if (player.NumGrenades > 0)
                {
                    ThrowGrenade(player, new Vector3(ox, oy, oz), new Vector3(dx, dy, dz), power);

                    if (!DashCMD.GetCVar<bool>("ch_infammo"))
                        player.NumGrenades--;
                }
            }
        }

        void R_SetBlock(NetConnection client, NetBuffer buffer, ushort numArgs)
        {
            int cx = buffer.ReadInt16();
            int cy = buffer.ReadInt16();
            int cz = buffer.ReadInt16();

            int bx = buffer.ReadUInt16();
            int by = buffer.ReadUInt16();
            int bz = buffer.ReadUInt16();

            byte r = buffer.ReadByte();
            byte g = buffer.ReadByte();
            byte b = buffer.ReadByte();
            byte d = buffer.ReadByte();

            bool placement = buffer.ReadBool();

            ServerMPPlayer player;
            if (players.TryGetValue(client, out player))
            {
                IndexPosition chunkPos = new IndexPosition(cx, cy, cz);
                IndexPosition blockPos = new IndexPosition(bx, by, bz);

                Block block = new Block(new Nybble2(d), r, g, b);
                // Validate modification
                float distToBlock = (player.GetCamera().Position - Chunk.ChunkBlockToWorldCoords(chunkPos, blockPos)).Length;
                if (!placement/* && distToBlock <= Spade.MODIFY_RANGE*/)
                {
                    SetBlock(chunkPos, blockPos, block, placement);

                    if (block == Block.AIR)
                        player.NumBlocks++;
                }
                else if (placement && /*distToBlock <= BlockItem.PLACE_RANGE
                    &&*/ player.NumBlocks > 0)
                {
                    SetBlock(chunkPos, blockPos, block, placement);

                    if (!DashCMD.GetCVar<bool>("ch_infammo"))
                        player.NumBlocks--;
                }
            }
        }

        public override PlayerRaycastResult RaycastPlayers(Ray ray, float maxDist = float.MaxValue, params Player[] ignore)
        {
            bool sv_impacts = DashCMD.GetCVar<bool>("sv_impacts");
            bool sv_hitboxes = DashCMD.GetCVar<bool>("sv_hitboxes");

            ServerMPPlayer hitPlayer = null;
            float? hitPlayerAt = null;

            foreach (ServerMPPlayer otherPlayer in players.Values)
            {
                // Make sure we aren't ignoring this player
                if (ignore.Length == 0 || !Array.Exists(ignore, (x => x == otherPlayer)))
                {
                    Vector3 otherPlayerPosition;
                    float otherPlayerCamYaw;
                    if (rollbackTime.HasValue)
                    {
                        // We are applying rollback, so find the players old transform
                        int otherPlayerPing = DashCMD.GetCVar<bool>("rp_usetargetping") ? otherPlayer.StateInfo.Owner.Stats.Ping : 0;
                        int rollbackFrame = MathHelper.Clamp(otherPlayerPing + rollbackTime.Value, 0, 1000);

                        if (sv_hitboxes)
                            DashCMD.WriteLine("[RB] Rolling back bullet-player transform by {0}ms [target ping: {1}ms]",
                                ConsoleColor.Green, rollbackTime, otherPlayerPing);

                        PlayerTransform otherPlayerTransform = otherPlayer.RollbackTransform(Environment.TickCount - rollbackFrame);
                        otherPlayerPosition = otherPlayerTransform.Position;
                        otherPlayerCamYaw = otherPlayerTransform.CameraYaw;
                    }
                    else
                    {
                        // No rollback currently, use current transform
                        otherPlayerPosition = otherPlayer.Transform.Position;
                        otherPlayerCamYaw = otherPlayer.GetCamera().Yaw;
                    }

                    if (sv_hitboxes)
                        channel.FireEventForAllConnections("Client_RolledBackServerPlayer",
                            otherPlayerPosition.X, otherPlayerPosition.Y,
                            otherPlayerPosition.Z, (byte)otherPlayer.Team);

                    Ray newRay = new Ray(ray.Origin - otherPlayerPosition, ray.Direction);

                    float? dist;
                    // Check for intersection
                    if (newRay.Intersects(otherPlayer.GetOrientatedBoundingBox(otherPlayerCamYaw), out dist))
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

        public override void FireBullet(Player _player, Vector3 origin, Vector3 dir, Vector3 recoil,
            int blockDamage, float playerDamage, float maxDist = float.MaxValue)
        {
            ServerMPPlayer player = (ServerMPPlayer)_player;

            bool sv_impacts = DashCMD.GetCVar<bool>("sv_impacts");
            bool sv_hitboxes = DashCMD.GetCVar<bool>("sv_hitboxes");

            int shooterPing = player.StateInfo.Owner.Stats.Ping;
            int rollbackOffset = DashCMD.GetCVar<int>("rp_rollback_offset");
            int bulletDelta = player.LastBulletDeltaTime;

            if (sv_hitboxes)
                DashCMD.WriteLine("[RB] Starting bullet {0}ms delta with {1}ms offset",
                    ConsoleColor.Green, bulletDelta, rollbackOffset);
            
            // Raycast
            BeginRollback(shooterPing + bulletDelta + rollbackOffset);
            Ray ray = new Ray(origin, dir + recoil);
            WorldRaycastResult result = Raycast(ray, true, maxDist, player);
            EndRollback();

            // Handle intersection
            if (result.Intersects)
            {
                if (result.HitTerrain)
                {
                    // Damage terrain
                    TerrainRaycastResult tResult = result.TerrainResult;

                    if (blockDamage > 0)
                        tResult.Chunk.DamageBlock(tResult.BlockIndex.Value, blockDamage);
                }
                else if (result.HitPlayer)
                {
                    // Damage player
                    PlayerRaycastResult pResult = result.PlayerResult;

                    bool ff = DashCMD.GetCVar<bool>("mp_friendlyfire");
                    bool infh = DashCMD.GetCVar<bool>("ch_infhealth");

                    if (!infh && (ff || player.Team != pResult.Player.Team))
                    {
                        DamagePlayer(player, player.ItemManager.SelectedItem.GetType().Name, (ServerMPPlayer)pResult.Player,
                            playerDamage, origin);

                        if (sv_impacts)
                            DashCMD.WriteLine("[IMP] Hit player for {0} damage. Health After: {1}", playerDamage, pResult.Player.Health);
                    }
                }

                ImpactAt(result.IntersectionPosition.Value);
            }
        }

        public void DamagePlayer(ServerMPPlayer attacker, string weapon, ServerMPPlayer hitPlayer, 
            float damage, Vector3 attackOrigin)
        {
            if (hitPlayer.Health > 0)
            {
                // Deal damage
                hitPlayer.Damage(attacker, damage, weapon);

                // Send hit feedback to hitPlayer
                NetConnectionSnapshotState state;
                PlayerSnapshot pSnapshot;
                if (snapshotComponent.ConnectionStates.TryGetValue(hitPlayer.StateInfo.Owner, out state))
                    if (state.WorldSnapshot.TryGetPlayer(hitPlayer.StateInfo.Id, out pSnapshot))
                        pSnapshot.HitFeedbackSnapshot.Hits.Add(attackOrigin);

                // Send hit feedback to attacker
                if (snapshotComponent.ConnectionStates.TryGetValue(attacker.StateInfo.Owner, out state))
                    if (state.WorldSnapshot.TryGetPlayer(attacker.StateInfo.Id, out pSnapshot))
                        pSnapshot.HitEnemy++;
            }
        }

        void ImpactAt(Vector3 origin)
        {
            if (DashCMD.GetCVar<bool>("sv_impacts"))
                channel.FireEventForAllConnections("Client_ServerImpact", origin.X, origin.Y, origin.Z);
        }

        public override Block SetBlock(IndexPosition chunkIndex, IndexPosition blockPos, Block block, bool placement)
        {
            return Terrain.Chunks[chunkIndex].SetBlock(block, blockPos);
        }

        public override void ThrowGrenade(Player owner, Vector3 origin, Vector3 dir, float power)
        {
            GrenadeEntity ent = new GrenadeEntity(owner, origin, dir, this, power);
            grenades.Add(ent);
            AddGameObject(ent);

            channel.FireEventForAllConnections("Client_ThrowGrenade", origin.X, origin.Y, origin.Z, 
                dir.X, dir.Y, dir.Z, power);
        }

        public override void ShootMelon(Player owner, Vector3 origin, Vector3 dir)
        {
            MelonEntity ent = new MelonEntity(owner, origin, dir, this);
            melons.Add(ent);
            AddGameObject(ent);
        }

        public override void Explode(Explosion explosion)
        {
            Vector3 origin = explosion.Origin;
            float radius = explosion.BlockRadius;

            IndexPosition cpos = FixedTerrain.WorldToChunkCoords(origin);
            IndexPosition blockPos = Chunk.WorldToBlockCoords(origin);
            blockPos -= cpos * Chunk.SIZE;

            Chunk chunk;
            if (Terrain.Chunks.TryGetValue(cpos, out chunk))
            {
                int blockRadius = (int)(radius / Block.CUBE_SIZE);
                for (int x = -blockRadius; x <= blockRadius; x++)
                    for (int y = -blockRadius; y <= blockRadius; y++)
                        for (int z = -blockRadius; z <= blockRadius; z++)
                        {
                            int nx = x + blockPos.X;
                            int ny = y + blockPos.Y;
                            int nz = z + blockPos.Z;

                            IndexPosition ncpos = chunk.WrapBlockCoords(ref nx, ref ny, ref nz);

                            if (!chunk.IsBlockCoordInRange(nx, ny, nz))
                                continue;
                            
                            Vector3 apos = Chunk.ChunkBlockToWorldCoords(ncpos, new IndexPosition(nx, ny, nz))
                                - Block.HALF_CUBE_3D_SIZE;

                            float dist = Maths.Distance(apos, origin);
                            if (dist > radius)
                                continue;

                            int damage = (int)(14 * (1f - (dist / radius)));

                            if (ncpos != cpos)
                            {
                                Chunk otherChunk;
                                if (Terrain.Chunks.TryGetValue(ncpos, out otherChunk))
                                    otherChunk.DamageBlock(new IndexPosition(nx, ny, nz), damage);
                            }
                            else
                                chunk.DamageBlock(new IndexPosition(nx, ny, nz), damage);
                        }
            }

            // Apply grenade damage
            bool ff = DashCMD.GetCVar<bool>("mp_friendlyfire");
            if (!DashCMD.GetCVar<bool>("ch_infhealth"))
                foreach (ServerMPPlayer player in players.Values)
                {
                    if (player.Health <= 0 || !ff && explosion.Owner.Team == player.Team && explosion.Owner != player)
                        continue;

                    PlayerRaycastResult eResult = RaycastPlayer(explosion.Origin, player, explosion.PlayerRadius);
                    if (eResult.Intersects)
                    {
                        /*
                            Curve:
                            max(min((fa/max(x,0)) - (fa/d), a), 0)
                            where f = falloff rate, a = max damage, d = max distance,
                                x = distance
                        */

                        //float damage = MathHelper.Clamp(
                        //    explosion.Damage / (eResult.IntersectionDistance.Value * explosion.DamageFalloff),
                        //    0, explosion.Damage);

                        float damage = explosion.Damage * (float)Math.Cos(eResult.IntersectionDistance.Value / ((2 * explosion.PlayerRadius) / Math.PI));

                        //float fa = explosion.DamageFalloff * explosion.Damage;
                       // float damage = MathHelper.Clamp((fa / eResult.IntersectionDistance.Value) - (fa / 200f), 0, explosion.Damage);

                        DamagePlayer((ServerMPPlayer)explosion.Owner, "Grenade", player, damage, origin);
                    }
                }

            radius = explosion.PlayerRadius;

            for (int i = 0; i < grenades.Count; i++)
            {
                GrenadeEntity grenade = grenades[i];

                Vector3 dirTo = grenade.Transform.Position - origin;
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

        public override void Update(float deltaTime)
        {
            foreach (KeyValuePair<NetConnection, ServerMPPlayer> pair in players)
            {
                ServerMPPlayer player = pair.Value;
                if (player.Health <= 0)
                {
                    player.OnKilled();

                    if (OnPlayerKilled != null)
                    {
                        if (player.LastDamage != null)
                            OnPlayerKilled(player.LastDamage);
                        else
                            OnPlayerKilled(new PlayerDamage(player, 0, "Unknown"));
                    }
                }
            }

            float timeAutoShift = DashCMD.GetCVar<float>("time_autoshift");
            TimeOfDay += timeAutoShift * deltaTime;

            base.Update(deltaTime);
        }
    }
}