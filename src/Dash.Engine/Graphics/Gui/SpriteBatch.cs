using Dash.Engine.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Dash.Engine.Graphics.Gui
{
    public class SpriteBatch : IDisposable
    {
        struct SpriteInfo
        {
            public uint Texture;
            public Rectanglei Rect;
            public Rectangle UV;
            public Color Color;
            public float Rotation;
            public Vector2 Origin;

            public SpriteInfo(uint tex, Rectanglei rect, Rectangle uv, Color color,
                float rotation, Vector2 origin)
            {
                Texture = tex;
                UV = uv;
                Rect = rect;
                Color = color;
                Rotation = rotation;
                Origin = origin;
            }
        }

        class Batch
        {
            public SpriteInfo[] Sprites;
            public int NumSprites;
            public Texture Texture;

            public Batch(SpriteInfo[] sprites, int numSprites)
            {
                Sprites = sprites;
                NumSprites = numSprites;
            }
        }

        List<Batch> batchedSprites;
        Queue<Batch> unusedBatches;
        Batch currentBatch;
        int bi;

        SpriteVertexBuffer spriteBuffer;

        float[] vertexBuffer;
        float[] uvBuffer;
        byte[] colorBuffer;

        int vbi;
        int ubi;
        int cbi;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Matrix4 ViewMatrix = Matrix4.Identity;
        public Matrix4 TransformationMatrix = Matrix4.Identity;

        public SpriteBatch(int screenWidth, int screenHeight)
        {
            Width = screenWidth;
            Height = screenHeight;

            batchedSprites = new List<Batch>();
            unusedBatches = new Queue<Batch>();
            currentBatch = new Batch(new SpriteInfo[SpriteVertexBuffer.MAX_SPRITES], 0);
            spriteBuffer = new SpriteVertexBuffer();

            vertexBuffer = new float[SpriteVertexBuffer.MAX_SPRITES * 8];
            uvBuffer = new float[SpriteVertexBuffer.MAX_SPRITES * 8];
            colorBuffer = new byte[SpriteVertexBuffer.MAX_SPRITES * 16];
        }

        public void Resize(int screenWidth, int screenHeight)
        {
            Width = screenWidth;
            Height = screenHeight;
        }

        public void Draw(Texture tex, Rectangle rect, Color color)
        {
            Draw(tex, rect, null, color, 0, Vector2.Zero);
        }

        public void Draw(Texture tex, Rectangle rect, Rectangle? clip, Color color)
        {
            Draw(tex, rect, clip, color, 0, Vector2.Zero);
        }

        public void Draw(Texture tex, Rectangle rect, Rectangle? clip, Color color, float rotation, Vector2 origin)
        {
            Rectanglei dstRect = rect.ToRectaglei();
            Rectanglei clipI = clip.HasValue ? clip.Value.ToRectaglei() : new Rectanglei(0, 0, tex.Width, tex.Height);

            float sheetWidth = tex.Width;
            float sheetHeight = tex.Height;

            float uvX = clipI.X / sheetWidth;
            float uvY = (sheetHeight - clipI.Bottom) / sheetHeight;
            float uvR = clipI.Right / sheetWidth;
            float uvB = (sheetHeight - clipI.Y) / sheetHeight;

            BatchSprite(tex, dstRect, new Rectangle(uvX, uvY, uvR, uvB), color, rotation, origin);
        }

        void BatchSprite(Texture tex, Rectanglei rect, Rectangle uv, Color color, float rotation, Vector2 origin)
        {
            if (currentBatch.NumSprites >= SpriteVertexBuffer.MAX_SPRITES || (currentBatch.Texture != null && currentBatch.Texture != tex))
            {
                batchedSprites.Add(currentBatch);
                currentBatch = GetBatch();
                bi = 0;
            }

            SpriteInfo si = new SpriteInfo(tex.Id, rect, uv, color, rotation, origin);
            if (currentBatch.Texture == null)
                currentBatch.Texture = tex;
            currentBatch.Sprites[bi++] = si;
            currentBatch.NumSprites++;
        }

        Vector2 RotatePoint(Vector2 o, Vector2 p, float r)
        {
            float s = (float)Math.Sin(r);
            float c = (float)Math.Cos(r);

            p.X -= o.X;
            p.Y -= o.Y;

            float xNew = p.X * c - p.Y * s;
            float yNew = p.X * s + p.Y * c;

            return new Vector2(o.X + xNew, o.Y + yNew);
        }

        public void Render(Shader shader)
        {
            shader.LoadMatrix4("viewMatrix", ViewMatrix);
            shader.LoadMatrix4("transformationMatrix", TransformationMatrix);

            if (bi > 0)
                batchedSprites.Add(currentBatch);

            if (batchedSprites.Count == 0)
                return;

            vbi = ubi = cbi = 0;

            spriteBuffer.Bind();
            shader.EnableAttributes();

            GL.ActiveTexture(TextureUnit.Texture0);

            for (int i = 0; i < batchedSprites.Count; i++)
            {
                Batch batch = batchedSprites[i];
                batch.Texture.Bind();

                for (int k = 0; k < batch.NumSprites; k++)
                {
                    SpriteInfo si = batch.Sprites[k];
                    si.Rect.X -= Width / 2;
                    si.Rect.Y = -si.Rect.Y + (Height / 2) - si.Rect.Height;

                    Vector2 br = new Vector2(si.Rect.Right, si.Rect.Bottom);
                    Vector2 bl = new Vector2(si.Rect.X, si.Rect.Bottom);
                    Vector2 tl = new Vector2(si.Rect.X, si.Rect.Y);
                    Vector2 tr = new Vector2(si.Rect.Right, si.Rect.Y);

                    if (si.Rotation != 0)
                    {
                        Vector2 o = si.Origin + si.Rect.Location;
                        br = RotatePoint(o, br, si.Rotation);
                        bl = RotatePoint(o, bl, si.Rotation);
                        tl = RotatePoint(o, tl, si.Rotation);
                        tr = RotatePoint(o, tr, si.Rotation);
                    }

                    AddVertex(br.X, br.Y, si.UV.Width, si.UV.Height, si.Color.R, si.Color.G, si.Color.B, si.Color.A);
                    AddVertex(bl.X, bl.Y, si.UV.X, si.UV.Height, si.Color.R, si.Color.G, si.Color.B, si.Color.A);
                    AddVertex(tl.X, tl.Y, si.UV.X, si.UV.Y, si.Color.R, si.Color.G, si.Color.B, si.Color.A);
                    AddVertex(tr.X, tr.Y, si.UV.Width, si.UV.Y, si.Color.R, si.Color.G, si.Color.B, si.Color.A);
                }

                spriteBuffer.UpdateBuffers(batch.NumSprites, vertexBuffer, uvBuffer, colorBuffer);
                spriteBuffer.Render(batch.NumSprites);

                vbi = ubi = cbi = 0;

                batch.NumSprites = 0;
                batch.Texture = null;
                unusedBatches.Enqueue(batch);
            }

            spriteBuffer.Unbind();

            batchedSprites.Clear();
            bi = 0;
            currentBatch = GetBatch();
        }

        Batch GetBatch()
        {
            if (unusedBatches.Count > 0)
                return unusedBatches.Dequeue();
            else
                return new Batch(new SpriteInfo[SpriteVertexBuffer.MAX_SPRITES], 0);
        }

        void AddVertex(float x, float y, float u, float v, byte r, byte g, byte b, byte a)
        {
            vertexBuffer[vbi++] = x;
            vertexBuffer[vbi++] = y;

            uvBuffer[ubi++] = u;
            uvBuffer[ubi++] = v;

            colorBuffer[cbi++] = r;
            colorBuffer[cbi++] = g;
            colorBuffer[cbi++] = b;
            colorBuffer[cbi++] = a;
        }

        public void Dispose()
        {
            spriteBuffer.Dispose();
        }
    }
}
