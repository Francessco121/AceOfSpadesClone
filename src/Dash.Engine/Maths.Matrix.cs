/* Maths.Matrix.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public static partial class Maths
    {
        public static Matrix4 CreateTransformationMatrix(Vector3 translation, float rx, float ry, float rz, float scale)
        {
            return Matrix4.CreateScale(scale)
                * Matrix4.CreateRotationX(MathHelper.ToRadians(rx))
                * Matrix4.CreateRotationY(MathHelper.ToRadians(ry))
                * Matrix4.CreateRotationZ(MathHelper.ToRadians(rz))
                * Matrix4.CreateTranslation(translation);
        }

        public static Matrix4 CreateTransformationMatrix(Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            return Matrix4.CreateScale(scale)
                * Matrix4.CreateRotationX(MathHelper.ToRadians(rotation.X))
                * Matrix4.CreateRotationY(MathHelper.ToRadians(rotation.Y))
                * Matrix4.CreateRotationZ(MathHelper.ToRadians(rotation.Z))
                * Matrix4.CreateTranslation(translation);
        }

        public static Matrix4 CreateTransformationMatrix(Vector2 translation, Vector2 scale)
        {
            return Matrix4.CreateScale(scale.X, scale.Y, 1)
               * Matrix4.CreateTranslation(translation.X, translation.Y, 0);
        }

        public static Matrix4 CreateRotationMatrix(Vector3 rotation)
        {
            return Matrix4.CreateRotationX(MathHelper.ToRadians(rotation.X))
                * Matrix4.CreateRotationY(MathHelper.ToRadians(rotation.Y))
                * Matrix4.CreateRotationZ(MathHelper.ToRadians(rotation.Z));
        }

        #region Extensions
        // http://roy-t.nl/index.php/2010/03/04/getting-the-left-forward-and-back-vectors-from-a-view-matrix-directly/
        // FOR VIEW MATRICES: Transposed values are incorrect!! That trick only worked for OpenTK, these are non-transpose indexed values.
        public static Vector3 Forward(this Matrix4 mat)
        {
            return -mat.Backward();
        }

        public static Vector3 Backward(this Matrix4 mat)
        {
            return new Vector3(mat[2, 0], mat[2, 1], mat[2, 2]);
        }

        public static Vector3 Right(this Matrix4 mat)
        {
            return new Vector3(mat[0, 0], mat[0, 1], mat[0, 2]);
        }

        public static Vector3 Left(this Matrix4 mat)
        {
            return -mat.Right();
        }

        public static Vector3 Up(this Matrix4 mat)
        {
            return new Vector3(mat[1, 0], mat[1, 1], mat[2, 2]);
        }

        public static Vector3 Down(this Matrix4 mat)
        {
            return -mat.Up();
        }
        #endregion
    }
}
