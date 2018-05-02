using Dash.Engine.Graphics.OpenGL;

/* RenderableRay.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class RenderableRay
    {
        public ColorMesh Mesh { get; private set; }
        public Ray Ray { get; private set; }
        public Color4 Color { get; set; }
        public float Length { get; private set; }

        public RenderableRay(Ray ray, Color4 color, float length)
        {
            this.Ray = ray;
            this.Color = color;
            this.Length = length;

            BuildMesh(ray, color, length);
        }

        public void Update(Ray ray, float length)
        {
            UpdateMesh(ray, length);
        }

        void UpdateMesh(Ray ray, float length)
        {
            Vector3 rayEnd = ray.Origin + ray.Direction * length;
            float[] verts = new float[]
            {
                ray.Origin.X, ray.Origin.Y, ray.Origin.Z,
                rayEnd.X, rayEnd.Y, rayEnd.Z
            };

            float[] colors = new float[]
            {
                Color.R, Color.G, Color.B, Color.A,
                Color.R, Color.G, Color.B, Color.A,
            };

            float[] normals = new float[]
            {
                0, 1, 0,
                0, 1, 0,
            };

            Mesh.Update(verts, new uint[] { 0, 1 }, colors, normals);
        }

        void BuildMesh(Ray ray, Color4 color, float length)
        {
            Vector3 rayEnd = ray.Origin + ray.Direction * length;
            float[] verts = new float[]
            {
                ray.Origin.X, ray.Origin.Y, ray.Origin.Z,
                rayEnd.X, rayEnd.Y, rayEnd.Z
            };

            float[] colors = new float[]
            {
                color.R, color.G, color.B, color.A,
                color.R, color.G, color.B, color.A,
            };

            float[] normals = new float[]
            {
                0, 1, 0,
                0, 1, 0,
            };

            Mesh = new ColorMesh(BufferUsageHint.DynamicDraw, verts, new uint[] { 0, 1 }, colors, normals);
        }
    }
}
