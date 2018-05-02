using System;
using System.Collections.Generic;

namespace Dash.Engine.Graphics
{
    public class Camera2D
    {
        public static Camera2D Active { get; private set; }

        public Vector2 Position;
        public Vector2 Scale
        {
            get { return scale; }
            set
            {
                if (value.X == 0 || value.Y == 0)
                    throw new ArgumentOutOfRangeException("value", "Camera scale cannot be zero on any axis!");

                scale = value;
            }
        }
        Vector2 scale = new Vector2(1, 1);

        public float Zoom
        {
            get { return nextZoom; }
            set
            {
                if (value == 0)
                    throw new ArgumentOutOfRangeException("value", "Camera zoom cannot be zero!");

                if (value != nextZoom)
                {
                    Vector2 focus = Viewport.AbsoluteCenter;
                    nextZoom = value;
                    CenterAt(focus);
                }

            }
        }
        float nextZoom = 1;

        public List<float> ZoomLevels;
        public int ZoomLevel
        {
            get { return zoomLevel; }
            set
            {
                zoomLevel = MathHelper.Clamp(value, 0, ZoomLevels.Count - 1);
                Zoom = ZoomLevels[zoomLevel];
            }
        }
        int zoomLevel;

        public Rectangle Viewport
        {
            get { return new Rectangle(Position.X, Position.Y, renderer.ScreenWidth / Scale.X, renderer.ScreenHeight / Scale.Y); }
        }

        public bool SmoothZoom = true;

        public bool AllowUserMovement;
        public float MovementSpeed = 100;
        public bool LockPositionToPixels;

        MasterRenderer renderer;

        public Camera2D(MasterRenderer renderer)
        {
            this.renderer = renderer;
            this.ZoomLevels = new List<float>();
        }

        public void MakeActive()
        {
            Active = this;
        }

        public Vector2 WorldToScreenCoords(Vector2 world)
        {
            return new Vector2(
                (world.X * Scale.X) - (Position.X * Scale.X),
                (world.Y * Scale.Y) - (Position.Y * Scale.Y));
        }

        public Vector2 ScreenToWorldCoords(Vector2 screen)
        {
            return new Vector2(
                ((screen.X / Scale.X) + Position.X),
                ((screen.Y / Scale.Y) + Position.Y));
        }

        public void CenterAt(Vector2 p)
        {
            Position.X = p.X - Viewport.Width / 2f;
            Position.Y = p.Y - Viewport.Height / 2f;
        }

        public void Update(float deltaTime)
        {
            if (AllowUserMovement)
            {
                if (Input.GetKey(Key.W)) Position.Y -= deltaTime * MovementSpeed;
                if (Input.GetKey(Key.S)) Position.Y += deltaTime * MovementSpeed;
                if (Input.GetKey(Key.A)) Position.X -= deltaTime * MovementSpeed;
                if (Input.GetKey(Key.D)) Position.X += deltaTime * MovementSpeed;

                if (Input.ScrollDeltaY != 0)
                    ZoomLevel += (Input.ScrollDeltaY < 0 ? 1 : -1);
            }

            if (SmoothZoom)
            {
                Vector2 focus = Viewport.AbsoluteCenter;
                scale.X = scale.Y = Interpolation.Linear(scale.X, nextZoom, 0.4f);
                CenterAt(focus);
            }
            else
                scale.X = scale.Y = nextZoom;
        }
    }
}
