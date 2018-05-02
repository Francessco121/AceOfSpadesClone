/* WorldShader.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class WorldShader : Shader
    {
        public WorldShader()
            : base("world.vert", "world.frag")
        { }

        public void LoadSpecularUniforms(float power, float intensity)
        {
            LoadFloat("specularPower", power);
            LoadFloat("specularIntensity", intensity);
        }

        public void LoadLights(LightList lights)
        {
            // Load light data
            int lc = 0;
            for (int i = 0; i < lights.Count; i++)
            {
                Light light = lights[i];
                if (light.Visible)
                {
                    LoadVector3(IndexedUniform("lightPosition", lc), light.Position);
                    LoadVector3(IndexedUniform("lightDirection", lc), light.Direction);
                    LoadColor3(IndexedUniform("lightColor", lc), light.Color);
                    LoadVector3(IndexedUniform("attenuation", lc), light.Attenuation);
                    LoadFloat(IndexedUniform("lightPower", lc), light.LightPower);
                    LoadFloat(IndexedUniform("lightRadius", lc), light.Radius);
                    LoadInt(IndexedUniform("lightTypes", lc), (int)light.Type);

                    lc++;
                }
            }

            LoadInt("numLights", lc);
        }

        protected override void BindAttributes()
        {
            // Connect position attribute
            base.BindAttribute(0, "position");
            // Bind texture coordinates
            base.BindAttribute(1, "color");
            // Bind normals
            base.BindAttribute(2, "normal");
        }

        protected override void ConnectTextureUnits()
        {
            LoadInt("shadowMap", 0);
            LoadInt("skyMap", 1);
        }
    }
}
