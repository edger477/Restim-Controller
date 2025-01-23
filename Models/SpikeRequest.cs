namespace RestimController.Models
{
    public class SpikeRequest
    {
        public string InstanceName { get; set; }
        public float Amount { get; set; }

        public int MillisecondsOn { get; set; }
        public int MillisecondsOff { get; set; }

        public int RepeatCount { get; set; }
    }
}
