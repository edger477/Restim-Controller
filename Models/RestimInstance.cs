namespace RestimController.Models
{
    public class RestimInstance
    {
        public bool Enabled { get; set; }
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Host { get; set; }
        public required int Port { get; set; }
        public float Volume { get; set; }
        public float NewVolume { get; set; }

        internal float? NewDelayedVolume { get; set; }
        internal DateTime? NewDelayedVolumeSetAt { get; set; }
        public float MaxVolume { get; set; }
        public float MaxSpike { get; set; }

        public Spiking? CurrentSpike { get; set; }
        public required string MasterVolumeTCodeAxis { get; set; }

        public string? ErrorMessage { get; set; }

        public required List<AxisData> Axes { get; set; } = new List<AxisData>();
        public bool IsConnected { get; internal set; }

        public class Spiking
        {

            public float Intensity { get; set; }

            public int OnTime { get; set; }
            public int OffTime { get; set; }

            public int ToRepeat { get; set; }
            public bool IsOn { get; set; }

            public DateTime? NextOn { get; set; }
            public DateTime? NextOff { get; set; }
        }
    }
}