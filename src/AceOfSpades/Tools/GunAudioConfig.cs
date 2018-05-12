namespace AceOfSpades.Tools
{
    public class GunAudioConfig
    {
        public string LocalFilepath;
        public string ReplicatedFilepath;
        public string FarFilepath;

        public float NearMaxDistance = 500f;
        public float FarMaxDistance = 1000f;

        public float FarMinDistance = 500f;

        public float LocalGain = 1f;
        public float ReplicatedGain = 1f;
    }
}
