using Dash.Engine.Graphics.OpenGL;
using System;

/* SkyboxRenderer.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class SkyboxRenderer : Renderer
    {
        SkyboxShader shader;

        #region Constants
        private const float SIZE = 10;
        private readonly float[] VERTICES = new float[] {        
            -SIZE,  SIZE, -SIZE,
            -SIZE, -SIZE, -SIZE,
            SIZE, -SIZE, -SIZE,
            SIZE, -SIZE, -SIZE,
            SIZE,  SIZE, -SIZE,
            -SIZE,  SIZE, -SIZE,

            -SIZE, -SIZE,  SIZE,
            -SIZE, -SIZE, -SIZE,
            -SIZE,  SIZE, -SIZE,
            -SIZE,  SIZE, -SIZE,
            -SIZE,  SIZE,  SIZE,
            -SIZE, -SIZE,  SIZE,

            SIZE, -SIZE, -SIZE,
            SIZE, -SIZE,  SIZE,
            SIZE,  SIZE,  SIZE,
            SIZE,  SIZE,  SIZE,
            SIZE,  SIZE, -SIZE,
            SIZE, -SIZE, -SIZE,

            -SIZE, -SIZE,  SIZE,
            -SIZE,  SIZE,  SIZE,
            SIZE,  SIZE,  SIZE,
            SIZE,  SIZE,  SIZE,
            SIZE, -SIZE,  SIZE,
            -SIZE, -SIZE,  SIZE,

            -SIZE,  SIZE, -SIZE,
            SIZE,  SIZE, -SIZE,
            SIZE,  SIZE,  SIZE,
            SIZE,  SIZE,  SIZE,
            -SIZE,  SIZE,  SIZE,
            -SIZE,  SIZE, -SIZE,

            -SIZE, -SIZE, -SIZE,
            -SIZE, -SIZE,  SIZE,
            SIZE, -SIZE, -SIZE,
            SIZE, -SIZE, -SIZE,
            -SIZE, -SIZE,  SIZE,
            SIZE, -SIZE,  SIZE
	    };
        #endregion

        SimpleMesh cube;
        public Texture skyMap;

        public float currentHour = 9.5f;

        public float skyMapOffset;

        Light sun;

        /*  For graph of f(x)=sin(x) and f(x)=cos(x)
            where time = [0, 24]

            22-2: No sun
            2-7, 16-22: sun-rise, sun-set
            7-17: day
            -- -- --
            where time = [0, 2pi] (y-axis)

            sun-rise: [0, pi/4]
            day: [pi/4, 1.32]
            sun-set: [1.32, pi]
            night: [pi, 2pi]
        */

        public SkyboxRenderer(MasterRenderer master)
            : base(master)
        {
            cube = new SimpleMesh(BufferUsageHint.StaticDraw, 3, VERTICES);
            skyMap = GLoader.LoadTexture("Textures/skyMap.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            shader = new SkyboxShader();
        }

        public void Render(float deltaTime, bool drawSun)
        {
            if (Master.Sun != null)
            {
                sun = Master.Sun;
                sun.LightPower = 1;
            }

            StateManager.Disable(EnableCap.DepthTest);

            shader.Start();
            shader.LoadMatrix4("projectionMatrix", Camera.Active.ProjectionMatrix);
            shader.LoadMatrix4("viewMatrix", Camera.Active.ViewMatrix.ClearTranslation());
            shader.LoadVector3("sunPosition", Master.Sun != null ? Master.Sun.Position : Vector3.Zero);
            //shader.LoadColor3("fogColor", master.FogColor);

            //currentHour += (deltaTime);
            if (currentHour > 24)
                currentHour -= 24;
            else if (currentHour < 0)
                currentHour += 24;

            // http://i.imgur.com/Uj45YN2.png
            float timeP = currentHour / 24;

            /*
                            timeP     timeT
            midnight:       0       | 0.75
            noon:           0.5     | 0.25
            mid-sun-set:    0.75    | 0.5
            mid-sun-rise:   0.25    | 0 
            */

            float timeT = timeP - 0.25f;
            if (timeT < 0) timeT += 1;
            else if (timeT > 1) timeT -= 1;

            timeT = timeT * MathHelper.TwoPi;

            float sunY = (float)Math.Sin(timeT);

           // Master.AmbientIntensity = MathHelper.Clamp(sunY * 0.75f, 0.01f, 0.45f);
           // Master.LightFalloff = MathHelper.Clamp(sunY + 0.6f, 0, 1.25f);
            //Master.ShadowVisibility = MathHelper.Clamp(sunY - 0.2f, 0, 0.75f);
            Master.ShadowVisibility = 1f;
            Master.LightFalloff = 1f;
            Master.AmbientIntensity = 0f;
            float shadowMinBias = 0.0005f;
            float shadowMaxBias = 0.0015f;
            float shadowBiasRange = shadowMaxBias - shadowMinBias;

            Master.ShadowBias = MathHelper.Clamp(
                (((1.4f - sunY) * shadowMaxBias) - shadowMinBias),
                shadowMinBias, shadowMaxBias);

            if (Master.Sun != null)
                sun.Position = new Vector3((float)Math.Cos(timeT), sunY, 0);

            // 12 - 12 = 0      / 12 = 0
            // 0 - 12 = -12     / 12 = -1
            // 24 - 12 = 12     / 12 = 1

            float n = Math.Abs((currentHour - 12) / 12) * 1.5f;

            float mapOff = MathHelper.Clamp(n * n, 0, 1);

            //Diagnostics.DashCMD.WriteLine("Hour: {0:N2}, Off: {1:N2}, Fade: {2:N2}", currentHour, mapOff, mapFade);

            skyMapOffset = mapOff;
            shader.LoadFloat("skyMapOffset", mapOff);
            shader.LoadFloat("skyMapFade", 1f);
            shader.LoadBool("renderSun", drawSun);

            GL.ActiveTexture(TextureUnit.Texture0);
            skyMap.Bind();

            cube.Bind();
            shader.EnableAttributes();

            if (Master.GlobalWireframe)
                StateManager.EnableWireframe();

            GL.DrawArrays(BeginMode.Triangles, 0, cube.VertexCount);

            StateManager.DisableWireframe();

            shader.DisableAttributes();
            GL.BindVertexArray(0);
            shader.Stop();

            StateManager.Enable(EnableCap.DepthTest);
        }

        public override void Dispose()
        {
            shader.Dispose();
        }
    }
}
