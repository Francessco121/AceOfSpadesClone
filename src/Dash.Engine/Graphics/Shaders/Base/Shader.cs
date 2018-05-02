using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics.OpenGL;
using System;
using System.Collections.Generic;

/* Shader.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public abstract class Shader : IDisposable
    {
        protected class Uniform
        {
            public object LastValue;
            public int Location;

            public Uniform(int location)
            {
                this.Location = location;
            }

            public bool ValueChanged(object newValue)
            {
                bool changed = newValue != LastValue;
                if (changed)
                    LastValue = newValue;
                return changed;
            }
        }

        public static bool ThrowOnUnknownUniform = false;

        public ShaderProgram Program { get; private set; }
        public string Name { get; private set; }
        Dictionary<string, Uniform> uniforms;
        HashSet<int> attributeLocations;

        public Shader(string vertexFilePath, string fragmentFilePath, string[] vertexLibs = null, string[] fragLibs = null)
        {
            this.Name = this.GetType().Name;
            uniforms = new Dictionary<string, Uniform>();
            attributeLocations = new HashSet<int>();

            // Load shaders from file
            int numShaders = 2 + (vertexLibs != null ? vertexLibs.Length : 0) + (fragLibs != null ? fragLibs.Length : 0);
            GLShader[] shaders = new GLShader[numShaders];

            int shaderI = 0;
            // Main shaders
            shaders[shaderI++] = GLoader.LoadShader(vertexFilePath, ShaderType.VertexShader);
            shaders[shaderI++] = GLoader.LoadShader(fragmentFilePath, ShaderType.FragmentShader);

            // Libraries
            if (vertexLibs != null)
            {
                for (int i = 0; i < vertexLibs.Length; i++)
                    shaders[shaderI++] = GLoader.LoadShader(vertexLibs[i], ShaderType.VertexShader);
            }
            if (fragLibs != null)
            {
                for (int i = 0; i < fragLibs.Length; i++)
                    shaders[shaderI++] = GLoader.LoadShader(fragLibs[i], ShaderType.FragmentShader);
            }

            // Create program
            Program = GLoader.CreateProgram(shaders);

            // Bind attributes
            this.BindAttributes();

            // Link Program
            GL.LinkProgram(Program.ProgramId);

            // Connect texture units
            Start();
            this.ConnectTextureUnits();
            Stop();
            
            // Validate the program
            GL.ValidateProgram(Program.ProgramId);
            int state = GL.GetProgram(Program.ProgramId, ProgramParameter.ValidateStatus);
            if (state == 0)
            {
                string programState = GL.GetProgramInfoLog(Program.ProgramId);
                throw new GPUResourceException(String.Format("Program Failed to Validate. Reason: {0}", programState));
            }
        }

        public void Start()
        {
            Program.Use();
        }

        public void Stop()
        {
            GL.UseProgram(0);
        }

        public void Dispose()
        {
            Stop();
            Program.Dispose();
            uniforms.Clear();
        }

        public void EnableAttributes()
        {
            foreach (int attr in attributeLocations)
                GL.EnableVertexAttribArray(attr);
        }

        public void DisableAttributes()
        {
            foreach (int attr in attributeLocations)
                GL.DisableVertexAttribArray(attr);
        }

        protected abstract void ConnectTextureUnits();
        protected abstract void BindAttributes();
        protected void BindAttribute(int attr, string name)
        {
            // Bind attribute location
            GL.BindAttribLocation(Program.ProgramId, attr, name);
            attributeLocations.Add(attr);
        }
        protected string IndexedUniform(string name, int index)
        {
            return String.Format("{0}[{1}]", name, index);
        }
        protected Uniform GetUniform(string name)
        {
            Uniform uniform;
            // See if a quick link exists
            if (uniforms.TryGetValue(name, out uniform))
                return uniform;
            else
            {
                // Return the uniform id of the effect's program
                int location = GL.GetUniformLocation(Program.ProgramId, name);
                if (location == -1)
                {
                    string msg = String.Format("Uniform {0} does not exist in the shader {1}", name, Name);
                    if (ThrowOnUnknownUniform)
                        throw new GPUResourceException(msg);
                    else
                        DashCMD.WriteWarning(msg);
                }
                // Add a quick link
                uniform = new Uniform(location);
                uniforms.Add(name, uniform);
                return uniform;
            }
        }

        // // // Uniform Loaders // // //
        // // These apply delta changes only // /
        public void LoadFloat(string uniformName, float value)
        {
            Uniform uniform = GetUniform(uniformName);
            if (uniform.ValueChanged(value))
                GL.Uniform1f(uniform.Location, value);
        }

        public void LoadInt(string uniformName, int value)
        {
            Uniform uniform = GetUniform(uniformName);
            if (uniform.ValueChanged(value))
                GL.Uniform1i(uniform.Location, value);
        }

        public void LoadBool(string uniformName, bool value)
        {
            Uniform uniform = GetUniform(uniformName);
            if (uniform.ValueChanged(value))
                GL.Uniform1i(uniform.Location, value ? 1 : 0);
        }

        public void LoadVector2(string uniformName, Vector2 vec2)
        {
            Uniform uniform = GetUniform(uniformName);
            if (uniform.ValueChanged(vec2))
                GL.Uniform2f(uniform.Location, ref vec2);
        }

        public void LoadVector3(string uniformName, Vector3 vec3)
        {
            Uniform uniform = GetUniform(uniformName);
            if (uniform.ValueChanged(vec3))
                GL.Uniform3f(uniform.Location, ref vec3);
        }

        public void LoadVector4(string uniformName, Vector4 vec4)
        {
            Uniform uniform = GetUniform(uniformName);
            if (uniform.ValueChanged(vec4))
                GL.Uniform4f(uniform.Location, ref vec4);
        }

        public void LoadColor3(string uniformName, Color4 color)
        {
            LoadVector3(uniformName, new Vector3(color.R, color.G, color.B));
        }

        public void LoadColor3(string uniformName, Color color)
        {
            LoadVector3(uniformName, new Vector3(color.R / 255f, color.G / 255f, color.B / 255f));
        }

        public void LoadColor4(string uniformName, Color4 color)
        {
            LoadVector4(uniformName, new Vector4(color.R, color.G, color.B, color.A));
        }

        public void LoadColor4(string uniformName, Color color)
        {
            LoadVector4(uniformName, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f));
        }

        public void LoadMatrix4(string uniformName, Matrix4 mat4)
        {
            Uniform uniform = GetUniform(uniformName);
            if (uniform.ValueChanged(mat4))
                GL.UniformMatrix4fv(uniform.Location, mat4);
        }
    }
}
