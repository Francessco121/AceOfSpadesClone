﻿using AceOfSpades.Characters;
using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Audio;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;
using Dash.Engine.Physics;
using System;

/* GrenadeEntity.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public class GrenadeEntity : GameObject
    {
        public bool IsDed { get; private set; }
        public PhysicsBodyComponent PhysicsBody { get; }

        const float BLOCK_RADIUS = 25;
        const float PLAYER_RADIUS = 50;
        const float DAMAGE = 150;
        const float DAMAGE_FALLOFF = 0.125f;
        const float KABOOM_DELAY = 2f;

        float timer = KABOOM_DELAY;
        World world;
        Player owner;
        VoxelRenderComponent renderer;

        readonly AudioSource bounceAudioSource;

        public GrenadeEntity(Player owner, Vector3 position, Vector3 velocity, World world, float throwPower) 
            : base(position - new Vector3(0.75f))
        {
            this.owner = owner;
            this.world = world;

            // Setup physics
            PhysicsBody = new PhysicsBodyComponent(new Vector3(1.5f), 0.0001f);
            AddComponent(PhysicsBody);

            PhysicsBody.Velocity = velocity * throwPower;

            PhysicsBody.CanCollideWithSoft = false;
            PhysicsBody.BounceOnWallCollision = true;
            PhysicsBody.BounceOnVerticalCollision = true;
            PhysicsBody.VerticalBounceFalloff = 0.8f;
            PhysicsBody.HorizontalBounceFalloff = 0.7f;
            PhysicsBody.Friction = 0.2f;

            // Setup renderer
            if (GlobalNetwork.IsClient)
            {
                renderer = new VoxelRenderComponent();
                AddComponent(renderer);

                renderer.VoxelObject = AssetManager.LoadVoxelObject("Models/grenade.aosm", BufferUsageHint.StaticDraw);

                AudioBuffer bounceAudioBuffer = AssetManager.LoadSound("Weapons/Grenade/Bounce.wav");

                if (bounceAudioBuffer != null)
                {
                    bounceAudioSource = new AudioSource(bounceAudioBuffer);
                    bounceAudioSource.MaxDistance = 200;
                    bounceAudioSource.Gain = 0.25f;
                }

                PhysicsBody.OnCollision += PhysicsBody_OnCollision;
            }
        }

        private void PhysicsBody_OnCollision(object sender, PhysicsBodyComponent e)
        {
            if (Math.Abs(PhysicsBody.Velocity.Y) > 1f)
            {
                bounceAudioSource?.Play();
            }
        }

        protected override void Update(float deltaTime)
        {
            if (!IsDed)
            {
                timer -= deltaTime;

                if (bounceAudioSource != null)
                    bounceAudioSource.Position = Transform.Position;

                if (timer <= 0)
                {
                    IsDed = true;
                    world.Explode(new Explosion(owner, Transform.Position, BLOCK_RADIUS, PLAYER_RADIUS, DAMAGE, DAMAGE_FALLOFF, "Grenade"));

                    if (!GlobalNetwork.IsServer)
                    {
                        bool isFar = (Camera.Active.Position - Transform.Position).Length >= 400;

                        AudioBuffer explodeBuffer = AssetManager.LoadSound(isFar ? "Weapons/Grenade/explode-far.wav" : "Weapons/Grenade/explode.wav");

                        if (explodeBuffer != null)
                        {
                            AudioSource explodeAudioSource = new AudioSource(explodeBuffer);
                            explodeAudioSource.MaxDistance = isFar ? 1200 : 700;
                            explodeAudioSource.Position = Transform.Position;

                            world.PlayWorldAudio(new WorldAudioSource(explodeAudioSource));
                        }
                    }

                    Dispose();
                }
            }

            base.Update(deltaTime);
        }

        protected override void Draw()
        {
            renderer.WorldMatrix = Transform.Matrix;
            base.Draw();
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                bounceAudioSource?.Dispose();
            }

            base.Dispose();
        }
    }
}
