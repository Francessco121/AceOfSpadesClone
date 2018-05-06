namespace AceOfSpades.Net
{
    public class NetworkClientMovement
    {
        public bool Sprint { get; set; }
        public bool Crouch { get; set; }
        public bool Jump { get; set; }
        public bool Walk { get; set; }
        public bool MoveForward { get; set; }
        public bool MoveBackward { get; set; }
        public bool MoveLeft { get; set; }
        public bool MoveRight { get; set; }

        public float CameraPitch { get; set; }
        public float CameraYaw { get; set; }

        public float Length { get; set; }
        public ushort Sequence { get; set; }
    }
}
