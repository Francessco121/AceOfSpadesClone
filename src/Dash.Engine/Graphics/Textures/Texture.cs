using Dash.Engine.Graphics.OpenGL;
using System;

/* Texture.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class Texture : IDisposable
    {
        public static Texture Blank { get; internal set; }

        public uint Id { get; private set; }
        public bool Bound { get; private set; }

        public TextureMagFilter MagFilter { get; private set; }
        public TextureMinFilter MinFilter { get; private set; }
        public TextureWrapMode WrapS { get; private set; }
        public TextureWrapMode WrapT { get; private set; }

        public bool HasMipmap { get; private set; }
        public int LODBias { get; private set; }

        public float AlphaCutOff = 0.5f;

        public bool IsAtlas = false;
        public int AtlasRows = 1;

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        /// <summary>
        /// Creates and allocates a new 
        /// texture with a OpenGL id.
        /// </summary>
        public Texture()
        {
            Id = GManager.GenTexture();
            Bind();
            ApplyParamPack(new TextureParamPack());
            Unbind();
        }

        /// <summary>
        /// Creates and allocates a new 
        /// texture with a OpenGL id.
        /// </summary>
        public Texture(TextureParamPack paramPack)
        {
            Id = GManager.GenTexture();
            Bind();
            ApplyParamPack(paramPack);
            Unbind();
        }

        public void Bind()
        {
            Bound = true;
            GL.BindTexture(TextureTarget.Texture2D, Id);
        }

        public void Unbind()
        {
            Bound = false;
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        void CheckBind()
        {
            if (!Bound)
                throw new InvalidOperationException("Cannot modify texture, it is not bound!");
        }

        public void ApplyParamPack(TextureParamPack texParams)
        {
            CheckBind();

            SetMinMag(texParams.MinFilter, texParams.MagFilter);
            SetWrapMode(texParams.WrapS, texParams.WrapT);
        }

        public void SetMinMag(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            CheckBind();

            MinFilter = minFilter;
            MagFilter = magFilter;

            // Setup texture paramters
            GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        }

        public void SetWrapMode(TextureWrapMode wrapS, TextureWrapMode wrapT)
        {
            CheckBind();

            WrapS = wrapS;
            WrapT = wrapT;

            GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapS);
            GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapT);
        }

        public void SetLODBias(int lod)
        {
            CheckBind();
            if (!HasMipmap)
                throw new InvalidOperationException("Cannot modify LOD bias, this texture has no mipmap!");

            LODBias = lod;
            GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, lod);
        }

        public void GenerateMipmap()
        {
            CheckBind();
            if (HasMipmap)
                throw new InvalidOperationException("Cannot generate mipmap, this texture already has one!");

            HasMipmap = true;
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public void SetData<T>(int width, int height, PixelType pixelType, PixelFormat pixelFormat, T[] data) 
            where T : struct
        {
            CheckBind();

            Width = width;
            Height = height;
            
            // Set the texture data
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                pixelFormat, pixelType, data);
        }

        public void SetData(int width, int height, PixelType pixelType, PixelFormat pixelFormat, IntPtr data)
        {
            CheckBind();

            Width = width;
            Height = height;

            // Set the texture data
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                pixelFormat, pixelType, data);
        }

        public void SetData(int width, int height, Color4[] data)
        {
            // Convert the color data to a readable array for OpenGL
            float[] finalData = new float[data.Length * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                finalData[i] = data[i].R;
                finalData[i + 1] = data[i].G;
                finalData[i + 2] = data[i].B;
                finalData[i + 3] = data[i].A;
            }

            SetData(width, height, PixelType.Float, PixelFormat.Rgba, finalData);
        }

        public void SetData(int width, int height, Color[] data)
        {
            // Convert the color data to a readable array for OpenGL
            byte[] finalData = new byte[data.Length * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                finalData[i] = data[i].R;
                finalData[i + 1] = data[i].G;
                finalData[i + 2] = data[i].B;
                finalData[i + 3] = data[i].A;
            }

            SetData(width, height, PixelType.UnsignedByte, PixelFormat.Rgba, finalData);
        }

        public void Dispose()
        {
            GManager.DeleteTexture(Id);
        }
    }
}
