/* SimpleCamera.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class SimpleCamera
    {
        public virtual float Pitch
        {
            get { return pitch; }
            set { pitch = value; }
        }
        public virtual float Yaw
        {
            get { return yaw; }
            set { yaw = value; }
        }
        public virtual float Roll
        {
            get { return roll; }
            set { roll = value; }
        }

        float pitch;
        float yaw;
        float roll;

        public Vector3 Position;
        public Vector3 PreViewOffset;
        public Vector3 PostViewOffset;
        public Vector3 OffsetPosition
        {
            get { return Position + PreViewOffset; }
        }

        public Matrix4 ViewMatrix { get; protected set; }
        public Vector3 LookVector { get; protected set; }

        public void ApplyRecoil(float vertical, float horizontal)
        {
            Pitch = MathHelper.Clamp(Pitch - vertical, -89.9f, 89.9f);
            Yaw += horizontal;
        }

        public virtual void Update(float deltaTime)
        {
            ViewMatrix = CreateViewMatrix();
            LookVector = TransformNormalXY(-Vector3.UnitZ);
        }

        public Vector3 TransformX(Vector3 vec)
        {
            vec = Vector3.Transform(vec, Matrix4.CreateRotationX(MathHelper.ToRadians(Pitch)));

            vec.X *= -1;
            vec.Y *= -1;

            return vec;
        }

        public Vector3 TransformY(Vector3 vec)
        {
            vec = Vector3.Transform(vec, Matrix4.CreateRotationY(MathHelper.ToRadians(Yaw)));

            vec.X *= -1;
            vec.Y *= -1;

            return vec;
        }

        public Vector3 TransformXY(Vector3 vec)
        {
            vec = Vector3.Transform(vec,
                Matrix4.CreateRotationX(MathHelper.ToRadians(Pitch))
                * Matrix4.CreateRotationY(MathHelper.ToRadians(Yaw)));

            vec.X *= -1;
            vec.Y *= -1;

            return vec;
        }

        public Vector3 TransformNormalX(Vector3 vec)
        {
            vec = Vector3.TransformNormal(vec, Matrix4.CreateRotationX(MathHelper.ToRadians(Pitch)));

            vec.X *= -1;
            vec.Y *= -1;

            return vec;
        }

        public Vector3 TransformNormalY(Vector3 vec)
        {
            vec = Vector3.TransformNormalInverse(vec, Matrix4.CreateRotationY(MathHelper.ToRadians(Yaw)));

            //vec.X *= -1;
            //vec.Y *= -1;

            return vec;
        }

        public Vector3 TransformNormalXY(Vector3 vec)
        {
            vec = Vector3.TransformNormal(vec,
                Matrix4.CreateRotationX(MathHelper.ToRadians(Pitch))
                * Matrix4.CreateRotationY(MathHelper.ToRadians(Yaw)));

            vec.X *= -1;
            vec.Y *= -1;

            return vec;
        }

        Matrix4 CreateViewMatrix()
        {
            return Matrix4.CreateTranslation(-(OffsetPosition + PostViewOffset))
                     * Matrix4.CreateRotationY(MathHelper.ToRadians(Yaw))
                     * Matrix4.CreateRotationX(MathHelper.ToRadians(Pitch))
                     * Matrix4.CreateRotationZ(MathHelper.ToRadians(Roll));
        }
    }
}
