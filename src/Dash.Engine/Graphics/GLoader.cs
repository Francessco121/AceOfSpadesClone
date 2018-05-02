using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

/* GLoader.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public static class GLoader
    {
        public static string RootDirectory = "Content";
        public static bool LogShaderCompilation = false;

        public static string GetContentRelativePath(string filePath)
        {
            return Path.Combine(RootDirectory, filePath);
        }

        static string LoadFileToString(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(string.Format("Could not find file \"{0}\"", path), path);

            return File.ReadAllText(path);
        }

        #region Textures and Cubemaps
        public static Texture LoadTexture(string filePath,
            TextureMinFilter minFilter = TextureMinFilter.Linear, TextureMagFilter magFilter = TextureMagFilter.Linear, bool keepPath = false)
        {
            // Get full path
            string actualPath = keepPath ? filePath : GetContentRelativePath(filePath);

            if (!File.Exists(actualPath))
                throw new FileNotFoundException(string.Format("Could not find texture file \"{0}\"", actualPath), actualPath);

            GLError.Begin();

            // Generate bitmap and get it's data
            Bitmap bitmap = new Bitmap(actualPath);
            BitmapData texData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            // Create texture id and bind it
            Texture texture = new Texture();
            texture.Bind();

            // Set the texture data, min mag, and mipmap lod
            texture.SetData(texData.Width, texData.Height, PixelType.UnsignedByte, 
                OpenGL.PixelFormat.Bgra, texData.Scan0);
            texture.GenerateMipmap();
            texture.SetLODBias(-1);
            texture.SetMinMag(minFilter, magFilter);

            // Unbind the texture
            texture.Unbind();

            // Unlock the bitmap file
            bitmap.UnlockBits(texData);

            ErrorCode err = GLError.End();
            if (err != ErrorCode.NoError)
                throw new Exception(string.Format("Failed to load texture. OpenGL Error: {0}", err));

            return texture;
        }

        public static Texture LoadBlankTexture(Color color)
        {
            // Creates a new texture that is just a white square with the dimensions 1x1.
            Texture texture = new Texture();
            texture.Bind();
            texture.SetData<byte>(1, 1, PixelType.UnsignedByte, OpenGL.PixelFormat.Rgba, 
                new byte[] { color.R, color.G, color.B, color.A });
            texture.SetMinMag(TextureMinFilter.Linear, TextureMagFilter.Linear);
            texture.Unbind();

            return texture;
        }

        /// <summary>
        /// Loads a cubemap from 6 texture files
        /// <para>Files must be in the order: right, left, top, bottom, back, front!</para>
        /// </summary>
        public static CubeMap LoadCubeMap(string textureDir, string[] textureNames)
        {
            if (textureNames.Length != 6)
                throw new GLoaderException("Need 6 textures for a cube map!");

            // Generate the bind the new texture
            CubeMap cubemap = new CubeMap();
            GL.ActiveTexture(TextureUnit.Texture0);
            cubemap.Bind();

            // Load each face of the cubemap
            for (int i = 0; i < textureNames.Length; i++)
            {
                string actualPath = GetContentRelativePath(Path.Combine(textureDir, textureNames[i]));

                // Load the image
                Bitmap bitmap = new Bitmap(actualPath);
                BitmapData texData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                // Set it in OpenGL
                GL.TexImage2D(TextureTarget.TextureCubeMap + i, 0, PixelInternalFormat.Rgba, texData.Width, texData.Height, 0,
                    OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, texData.Scan0);

                // Release the file
                bitmap.UnlockBits(texData);
            }

            // Set the min and mag filters
            cubemap.SetMinMag(TextureMinFilter.Linear, TextureMagFilter.Linear);
            // Set the wrapping
            cubemap.SetWrap(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            cubemap.Unbind();
            return cubemap;
        }
        #endregion

        #region Shaders and Programs
        public static ShaderProgram CreateProgram(params GLShader[] shaders)
        {
            // Create the program
            uint programId = GL.CreateProgram();

            // Create the shader program
            ShaderProgram program = new ShaderProgram(programId, shaders);
            GManager.AllocateProgram(program);
            return program;
        }

        /// <summary>
        /// Loads a shader from a file.
        /// <para>At this stage, there is no auto-cleanup for this shader.
        /// Auto-Cleanup takes place once this is loaded to a ShaderProgram.</para>
        /// </summary>
        public static GLShader LoadShader(string path, ShaderType type)
        {
            GLShader shader;
            string fileName = Path.GetFileName(path);

            // If shader is not found, load and compile it
            if (!GManager.TryGetShader(fileName, out shader))
            {
                path = Path.Combine(RootDirectory, "Shaders/", path);

                // Allocate the shader Id
                shader = GManager.CreateShader(fileName, type);
                uint shaderId = shader.Id;
                // Load the shader from the file
                string shaderCode = LoadFileToString(path);
                // Load the shader's sourcecode to the GL shader
                GL.ShaderSource(shaderId, shaderCode);
                // Compile the shader for the GPU
                GL.CompileShader(shaderId);

                // Check the compilation status
                int status = GL.GetShader(shaderId, ShaderParameter.CompileStatus);
                string log = GL.GetShaderInfoLog(shaderId);
                
                // Log
                if (status == 0)
                    throw new GLoaderException(String.Format("Failed to compile {0}. Reason: {1}", Path.GetFileName(path), log));
                else if (LogShaderCompilation)
                    if (string.IsNullOrWhiteSpace(log))
                        DashCMD.WriteStandard("Compiled {0} with id {1}.", Path.GetFileName(path), shaderId);
                    else
                        DashCMD.WriteStandard("Compiled {0} with id {1}. Status: {2}", Path.GetFileName(path), shaderId, log);
            }

            return shader;
        }
        #endregion
    }
}
