public class AppSettings
{
    public required string AuthKey { get; set; }
    public required AudioSessionConfig[] AudioSessions { get; set; }
}

public class AudioSessionConfig
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string DeviceName { get; set; }
    public float MaxVolume { get; set; }
    public float MaxPain { get; set; }
}
