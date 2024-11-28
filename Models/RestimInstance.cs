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
        internal DateTime? NewDelatedVolumeSetAt { get; set; }
        public float MaxVolume { get; set; }
        public float MaxSpike { get; set; }
        public required string MasterVolumeTCodeAxis { get; set; }

        public string? ErrorMessage { get; set; }

        public required List<AxisData> Axes { get; set; } = new List<AxisData>();
        public bool IsConnected { get; internal set; }
    }
}