namespace RestimController.Models
{
    public class SpikeRequest
    {
        public uint Id { get; set; }
        public string DeviceName { get; set; }
        public float Amount { get; set; }

        public int MillisecondsOn { get; set; }
        public int MillisecondsOff { get; set; }

        public int RepeatCount { get; set; }
    }
}
