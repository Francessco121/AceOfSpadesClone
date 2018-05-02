/* ShadowCamera.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class ShadowCamera
    {
        public static AxisAlignedBoundingBox FinalAABB;
        public static bool UseExperimental;

        public Vector3 Position;
        public Vector3 Target;

        public Matrix4 LightSpaceMatrix { get; private set; }

        public void Update()
        {
            Matrix4 viewMatrix, projectionMatrix;

            if (UseExperimental)
            {
                // Calculate corners of the active camera's frustum
                OldFrustum f = new OldFrustum(Camera.Active, true);

                Vector3[] shadowFrustum = new Vector3[8];
                shadowFrustum[0] = f.FBL;
                shadowFrustum[1] = f.FBR;
                shadowFrustum[2] = f.FTL;
                shadowFrustum[3] = f.FTR;
                shadowFrustum[4] = f.NBL;
                shadowFrustum[5] = f.NBR;
                shadowFrustum[6] = f.NTL;
                shadowFrustum[7] = f.NTR;

                int shadowRes = MasterRenderer.Instance.GFXSettings.ShadowResolution;
                float shadowTexel = 1f / shadowRes;

                // Transform each frustum corner into light space
                Vector3 position = new Vector3(
                    (int)(Position.X / shadowTexel) * shadowTexel,
                    (int)(Position.Y / shadowTexel) * shadowTexel,
                    (int)(Position.Z / shadowTexel) * shadowTexel);
                Vector3 target = new Vector3(
                    (int)(Target.X / shadowTexel) * shadowTexel,
                    (int)(Target.Y / shadowTexel) * shadowTexel,
                    (int)(Target.Z / shadowTexel) * shadowTexel);
                viewMatrix = Matrix4.LookAt(position, target, Vector3.UnitY);
                Matrix4 invLightviewMatrix = viewMatrix.Inverse();

                for (int i = 0; i < shadowFrustum.Length; i++)
                {
                    Vector3 transformedVertex = Vector3.Transform(shadowFrustum[i], invLightviewMatrix);
                    transformedVertex.X = (int)(transformedVertex.X / shadowTexel) * shadowTexel;
                    transformedVertex.Y = (int)(transformedVertex.Y / shadowTexel) * shadowTexel;
                    transformedVertex.Z = (int)(transformedVertex.Z / shadowTexel) * shadowTexel;
                    shadowFrustum[i] = transformedVertex;
                }

                // Calculate the boundingbox of the frustum
                AxisAlignedBoundingBox aabb = new AxisAlignedBoundingBox();
                for (int i = 0; i < shadowFrustum.Length; i++)
                    aabb.AddPoint(shadowFrustum[i]);

                float maxX = (aabb.Max.X - aabb.Min.X) / 2f;
                maxX = (int)(maxX / shadowTexel) * shadowTexel;
                float minX = -maxX;
                float maxY = (aabb.Max.Y - aabb.Min.Y) / 2f;
                maxY = (int)(maxY / shadowTexel) * shadowTexel;
                float minY = -maxY;
                float maxZ = (aabb.Max.Z - aabb.Min.Z) / 2f;
                maxZ = (int)(maxZ / shadowTexel) * shadowTexel;
                float minZ = -maxZ;

                //FinalAABB = new AxisAlignedBoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));

                // Create the projection matrix of the light from the boundingbox
                projectionMatrix = Matrix4.CreateOrthographicOffCenter(minX, maxX, minY, maxY, minZ, maxZ);
                // Matrix4 projectionMatrix = Matrix4.CreateOrthographicOffCenter(minExtent, maxExtent, minExtent, maxExtent, minExtent, maxExtent);
            }
            else
            {
                viewMatrix = Matrix4.LookAt(Position, Target, Vector3.UnitY);
                projectionMatrix = Matrix4.CreateOrthographicOffCenter(-1600, 1600, -1600, 1600, 10, 3000);
            }

            // And finally, calculate the light space matrix
            LightSpaceMatrix = viewMatrix * projectionMatrix;
        }
    }
}
