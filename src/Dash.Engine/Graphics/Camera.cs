using Dash.Engine.Audio;
using Dash.Engine.Graphics.Gui;
using Dash.Engine.Physics;
using System;

/* Camera.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public enum CameraMode
    {
        FPS, ArcBall, Lookat
    }

    public class Camera : SimpleCamera
    {
        public static Camera Active { get; private set; }

        public CameraMode Mode { get; private set; }

        public Vector3 Target = Vector3.Zero;

        public override float Pitch
        {
            get { return pitch; }
            set { targetPitch = value; }
        }
        public override float Yaw
        {
            get { return yaw; }
            set { targetYaw = value; }
        }
        public override float Roll
        {
            get { return roll; }
            set { targetRoll = value; }
        }

        public float ArcBallRadius
        {
            get { return arcRadius; }
            set { targetRadius = value; }
        }

        public float DefaultFOV = 70;
        public float FOV = 70;
        public float NearPlane = 1f;
        public float FarPlane = 3000f;
        public float AspectRatio;

        public float DefaultFPSMouseSensitivity = 0.12f;
        public float FPSMouseSensitivity = 0.12f;
        public float DefaultArcBallMouseSensitivity = 0.2f;
        public float ArcBallMouseSensitivity = 0.2f;

        public float[] Speeds = new float[2];

        public Matrix4 YViewMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }
        public Frustum ViewFrustum { get; private set; }
        public Ray MouseRay { get; private set; }

        public Transform LockedToTransform;
        public Vector3 FirstPersonLockOffset;

        public AudioListener AudioListener { get; }

        public bool NeedsResize;

        public bool AllowUserControl = true;
        public bool HoldM2ToLook = false;

        public bool SmoothCamera = false;

        MasterRenderer renderer;
        float pitch;
        float yaw;
        float roll;
        float arcRadius = 50;
        float targetPitch;
        float targetYaw;
        float targetRoll;
        float targetRadius;

        const float butterInterp = 0.4f;

        Vector3 lastPosition;

        public Camera(MasterRenderer renderer)
        {
            AspectRatio = (float)renderer.ScreenWidth / renderer.ScreenHeight;
            this.renderer = renderer;

            Speeds[0] = 2f;
            Speeds[1] = 5f;

            MouseRay = new Ray(Vector3.Zero, Vector3.UnitZ);
            Position = new Vector3(0, 400, 0);
            ViewFrustum = new Frustum();
            AudioListener = new AudioListener(Position);
            UpdateMatrices();
        }

        public void MakeActive()
        {
            Active = this;
            AudioListener.MakeActive();
        }

        public void SetMode(CameraMode newMode)
        {
            if (Mode == newMode) return;

            if (newMode == CameraMode.Lookat)
                throw new NotImplementedException("Camera lookat mode is not implemented");

            if (newMode == CameraMode.ArcBall)
            {
                Vector3 rotationVector = CalculateArcDirectionVector();
                Target = OffsetPosition + rotationVector;
            }

            Mode = newMode;
        }

        public void SetRotation(float yaw, float pitch, float roll = 0)
        {
            Yaw = yaw;
            Pitch = pitch;
            targetRoll = roll;
        }

        public Vector3 GetMouse3DPosition()
        {
            return MouseRay.Origin + (MouseRay.Direction * 2);
        }

        public Vector2 Project(Vector3 position)
        {
            Vector3 cameraSpace = Vector3.Transform(position, ViewMatrix);
            Vector3 clipSpace = Vector3.Transform(cameraSpace, ProjectionMatrix);
            return new Vector2(
                renderer.ScreenWidth * 0.5f + clipSpace.X * renderer.ScreenWidth * 0.5f / clipSpace.Z,
                renderer.ScreenHeight * 0.5f + -clipSpace.Y * renderer.ScreenHeight * 0.5f / clipSpace.Z);
        }

        Vector3 CalculateMouseRay()
        {
            // Viewport space
            float mx = Input.CursorX;
            float my = Input.CursorY;

            // To normal device space
            Vector2 normalCoords = GetNormalizedDeviceCoords(mx, my);

            // To homogeneous clip space
            Vector4 clipCoords = new Vector4(normalCoords.X, normalCoords.Y, -1f, 1f);

            // To eye space
            Vector4 eyeCoords = ClipToEyeCoords(clipCoords);

            // To world space
            Vector3 worldRay = EyeToWorldCoords(eyeCoords);

            return worldRay;
        }

        public Vector3 EyeToWorldCoords(Vector4 eyeCoords)
        {
            Matrix4 inverseView = ViewMatrix.Inverse();
            Vector4 worldCoords = Vector4.Transform(eyeCoords, inverseView);
            Vector3 worldCoordsFinal = new Vector3(worldCoords.X, worldCoords.Y, worldCoords.Z);
            worldCoordsFinal = worldCoordsFinal.Normalize();

            return worldCoordsFinal;
        }

        public Vector4 ClipToEyeCoords(Vector4 clipCoords)
        {
            Matrix4 invertedProjection = ProjectionMatrix.Inverse();
            Vector4 eyeCoords = Vector4.Transform(clipCoords, invertedProjection);

            return new Vector4(eyeCoords.X, eyeCoords.Y, -1f, 0f);
        }

        public Vector2 GetNormalizedDeviceCoords(float mx, float my)
        {
            float x = (2f * mx) / renderer.ScreenWidth - 1;
            float y = (2f * my) / renderer.ScreenHeight - 1;

            return new Vector2(x, -y);
        }

        public void OnResize(int width, int height)
        {
            AspectRatio = (float)width / height;
            NeedsResize = false;
        }

        public void SetTarget(Vector3 target)
        {
            Target = target;
            arcRadius = targetRadius = Maths.Distance(OffsetPosition, Target);
        }

        public override void Update(float deltaTime)
        {
            if (LockedToTransform != null)
            {
                if (Mode == CameraMode.ArcBall)
                {
                    Target.X = LockedToTransform.Position.X + FirstPersonLockOffset.X;
                    Target.Y = Interpolation.Linear(Target.Y, LockedToTransform.Position.Y + FirstPersonLockOffset.Y, 0.1f);
                    Target.Z = LockedToTransform.Position.Z + FirstPersonLockOffset.Z;
                }
                else if (Mode == CameraMode.FPS)
                {
                    Position.X = LockedToTransform.Position.X + FirstPersonLockOffset.X;
                    Position.Y = Interpolation.Linear(Position.Y, LockedToTransform.Position.Y + FirstPersonLockOffset.Y, 0.35f);
                    Position.Z = LockedToTransform.Position.Z + FirstPersonLockOffset.Z;
                }
            }
            else if (AllowUserControl && !GUISystem.HandledMouseInput)
            {
                // Calculate move vector
                float speed = Input.GetKey(Key.LeftShift) ? Speeds[1] : Speeds[0];
                Vector3 moveVec = Vector3.Zero;
                if (Input.GetKey(Key.W)) moveVec.Z += speed;
                if (Input.GetKey(Key.S)) moveVec.Z -= speed;
                if (Input.GetKey(Key.A)) moveVec.X -= speed;
                if (Input.GetKey(Key.D)) moveVec.X += speed;
                if (Input.GetKey(Key.E)) moveVec.Y += speed;
                if (Input.GetKey(Key.Q)) moveVec.Y -= speed;

                Vector3 transformedMove = TransformXY(moveVec);

                // Move the camera
                if (Mode == CameraMode.ArcBall)
                    Target -= transformedMove;
                else // TODO: correct movement for Lookat mode
                    Position -= transformedMove;
            }

            // Rotate camera
            if (AllowUserControl && !GUISystem.HandledMouseInput && (Input.GetMouseButton(MouseButton.Right) || (Mode == CameraMode.FPS && !HoldM2ToLook)))
            {
                float sensitivity = Mode == CameraMode.FPS ? FPSMouseSensitivity : ArcBallMouseSensitivity;

                float xDelta = Input.CursorDeltaX * sensitivity;
                float yDelta = Input.CursorDeltaY * sensitivity;

                if (xDelta != 0 || yDelta != 0)
                {
                    targetPitch = MathHelper.Clamp(targetPitch + yDelta, -89.9f, 89.9f);
                    targetYaw += xDelta;
                }
            }

            if (Mode == CameraMode.ArcBall)
            {
                targetRadius -= Input.ScrollDeltaY * 4f;
                if (targetRadius < 5) targetRadius = 5;
            }

            // Handle camera lerp
            if (SmoothCamera)
            {
                pitch = Interpolation.Linear(pitch, targetPitch, butterInterp);
                yaw = Interpolation.Linear(yaw, targetYaw, butterInterp);
                roll = Interpolation.Linear(roll, targetRoll, butterInterp);
                arcRadius = Interpolation.Linear(arcRadius, targetRadius, butterInterp);

                //if (Math.Abs(yaw - targetYaw) < 0.001f && (Math.Abs(yaw) > 360))
                //{
                //    yaw = yaw < 0 ? yaw + 360 : yaw - 360;
                //    targetYaw = yaw;
                //}
            }
            else
            {
                pitch = targetPitch;

                //if (targetYaw > 360)
                //    targetYaw -= 360;

                yaw = targetYaw;
                roll = targetRoll;
                arcRadius = targetRadius;
            }

            // Update matrices and pick ray
            UpdateMatrices();
            UpdateMouseRay();

            // Update look vector
            LookVector = TransformNormalXY(-Vector3.UnitZ);

            // Update audio listener
            Vector3 deltaPosition = Position - lastPosition;
            AudioListener.Position = Position;
            // AudioListener.Velocity = deltaPosition / deltaTime;
            AudioListener.SetOrientation(LookVector, Vector3.Up);

            lastPosition = Position;
        }

        void UpdateMouseRay()
        {
            Vector3 mouseRayDirection = CalculateMouseRay();
            MouseRay = new Ray(OffsetPosition, mouseRayDirection);
        }

        protected virtual void UpdateMatrices()
        {
            ViewMatrix = CreateViewMatrix();
            YViewMatrix = CreateYViewMatrix();
            ProjectionMatrix = CreateProjectionMatrix();
            ViewFrustum.UpdateFrustum(ViewMatrix * ProjectionMatrix);
        }

        Matrix4 CreateViewMatrix()
        {
            if (Mode == CameraMode.FPS)
                return Matrix4.CreateTranslation(-(OffsetPosition + PostViewOffset))
                     * Matrix4.CreateRotationY(MathHelper.ToRadians(Yaw))
                     * Matrix4.CreateRotationX(MathHelper.ToRadians(Pitch))
                     * Matrix4.CreateRotationZ(MathHelper.ToRadians(Roll));
            else if (Mode == CameraMode.ArcBall)
            {
                Position = Target - CalculateArcDirectionVector();
                return Matrix4.LookAt(Position, Target, Vector3.UnitY);
            }
            else
                return Matrix4.LookAt(Position, Target, Vector3.UnitY);
        }

        Matrix4 CreateYViewMatrix()
        {
            if (Mode == CameraMode.FPS)
                return Matrix4.CreateRotationY(MathHelper.ToRadians(Yaw));
            else
                return Matrix4.Identity;
        }

        Vector3 CalculateArcDirectionVector()
        {
            Matrix4 rotation = Matrix4.CreateRotationY(MathHelper.ToRadians(Yaw))
                     * Matrix4.CreateRotationX(MathHelper.ToRadians(Pitch));

            Vector3 direction = Vector3.TransformNormalInverse(new Vector3(0, 0, -1), rotation);
            direction *= ArcBallRadius;

            return direction;
        }

        Matrix4 CreateProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV),
                AspectRatio, NearPlane, FarPlane);
        }
    }
}
