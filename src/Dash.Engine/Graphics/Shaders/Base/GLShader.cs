using Dash.Engine.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Dash.Engine.Graphics
{
    public class GLShader : IDisposable
    {
        public uint Id { get; private set; }
        public ShaderType Type { get; private set; }
        public HashSet<ShaderProgram> AssociatedPrograms { get; private set; }

        public GLShader(uint id, ShaderType type)
        {
            this.Id = id;
            this.Type = type;
            AssociatedPrograms = new HashSet<ShaderProgram>();
        }

        public void AssociateProgram(ShaderProgram program)
        {
            AssociatedPrograms.Add(program);
        }

        public void UnassociateProgram(ShaderProgram program)
        {
            AssociatedPrograms.Remove(program);
        }

        public void Dispose()
        {
            GManager.DeleteShader(this);
        }
    }
}
