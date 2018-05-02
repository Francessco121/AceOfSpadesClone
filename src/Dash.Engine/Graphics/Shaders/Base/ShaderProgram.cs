using Dash.Engine.Graphics.OpenGL;
using System;

/* ShaderProgram.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class ShaderProgram : IDisposable
    {
        public uint ProgramId { get; private set; }
        public GLShader[] Shaders { get; private set; }
        public bool CleanedUp { get; private set; }

        public ShaderProgram(uint programId, GLShader[] shaders)
        {
            this.ProgramId = programId;
            this.Shaders = shaders;

            // Attach and associate shaders
            for (int i = 0; i < Shaders.Length; i++)
            {
                GLShader shader = Shaders[i];
                GL.AttachShader(ProgramId, shader.Id);
                shader.AssociateProgram(this);
            }
        }

        public void Use()
        {
            GL.UseProgram(ProgramId);
        }

        public void Dispose()
        {
            if (CleanedUp)
                return;

            CleanedUp = true;

            // Detach and unassociate shaders
            for (int i = 0; i < Shaders.Length; i++)
            {
                GLShader shader = Shaders[i];
                GL.DetachShader(ProgramId, shader.Id);
                shader.UnassociateProgram(this);
            }

            // Delete program
            GL.DeleteProgram(ProgramId);
        }
    }
}
