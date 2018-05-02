using Dash.Engine.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing.Text;

namespace Dash.Engine.Graphics
{
    public static class GManager
    {
        static List<uint> vaos = new List<uint>();
        static List<uint> vbos = new List<uint>();
        static List<uint> texs = new List<uint>();
        static List<uint> rbos = new List<uint>();
        static List<uint> fbos = new List<uint>();
        static Dictionary<string, GLShader> shaders = new Dictionary<string, GLShader>();
        static List<ShaderProgram> programs = new List<ShaderProgram>();

        #region Allocation
        public static void AllocateProgram(ShaderProgram program)
        {
            programs.Add(program);
        }

        public static bool TryGetShader(string originalPath, out GLShader shader)
        {
            return shaders.TryGetValue(originalPath, out shader);
        }

        public static GLShader CreateShader(string fileName, ShaderType type)
        {
            uint shaderId = GL.CreateShader(type);
            GLShader shader = new GLShader(shaderId, type);
            shaders.Add(fileName, shader);

            return shader;
        }

        public static uint GenVertexArray()
        {
            uint vao = GL.GenVertexArray();
            vaos.Add(vao);
            return vao;
        }

        public static uint GenBuffer()
        {
            uint vbo = GL.GenBuffer();
            vbos.Add(vbo);
            return vbo;
        }

        public static uint GenTexture()
        {
            uint texId = GL.GenTexture();
            texs.Add(texId);
            return texId;
        }

        public static uint GenRenderbuffer()
        {
            uint renderId = GL.GenRenderbuffer();
            rbos.Add(renderId);
            return renderId;
        }

        public static uint GenFramebuffer()
        {
            uint fbo = GL.GenFramebuffer();
            fbos.Add(fbo);
            return fbo;
        }
        #endregion

        #region Clean Up
        public static void DeleteVertexArray(uint vao)
        {
            vaos.Remove(vao);
            GL.DeleteVertexArray(vao);
        }

        public static void DeleteBuffer(uint vbo)
        {
            vbos.Remove(vbo);
            GL.DeleteBuffer(vbo);
        }

        public static void DeleteTexture(uint tex)
        {
            texs.Remove(tex);
            GL.DeleteTexture(tex);
        }

        public static void DeleteRenderbuffer(uint rb)
        {
            rbos.Remove(rb);
            GL.DeleteRenderbuffer(rb);
        }

        public static void DeleteFramebuffer(uint fbo)
        {
            fbos.Remove(fbo);
            GL.DeleteFramebuffer(fbo);
        }

        public static void DeleteShader(GLShader shader)
        {
            if (shader.AssociatedPrograms.Count > 0)
                throw new InvalidOperationException("Cannot delete shader, it is attached to alteast 1 program!");

            GL.DeleteShader(shader.Id);
        }
        #endregion

        public static void CleanUpFinal()
        {
            foreach (uint vbo in vbos)
                GL.DeleteBuffer(vbo);
            foreach (uint vao in vaos)
                GL.DeleteVertexArray(vao);
            foreach (uint rbo in rbos)
                GL.DeleteRenderbuffer(rbo);
            foreach (uint tex in texs)
                GL.DeleteTexture(tex);
            foreach (uint fbo in fbos)
                GL.DeleteFramebuffer(fbo);

            GL.UseProgram(0);
            foreach (ShaderProgram program in programs)
                program.Dispose();
            foreach (GLShader shader in shaders.Values)
                shader.Dispose();

            vaos.Clear();
            vbos.Clear();
            rbos.Clear();
            texs.Clear();
            shaders.Clear();
            programs.Clear();
        }
    }
}
