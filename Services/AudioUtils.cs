namespace VolumeControl.Services
{
    using Microsoft.Extensions.Options;
    using NAudio.CoreAudioApi;
    using System;
    using System.Linq;

    public class AudioUtils
    {
        private readonly AppSettings _appSettings;
        private readonly MMDeviceEnumerator _deviceEnumerator;
        private readonly AudioSessionHelper _audioSessionHelper;
        public AudioUtils(AppSettings appSettings)
        {
            _deviceEnumerator = new MMDeviceEnumerator();
            _audioSessionHelper = new AudioSessionHelper();
            _appSettings = appSettings;
        }

        public List<Device> GetDevices()
        {
            var audioSessions = _appSettings.AudioSessions.ToList();
            var sessions = _audioSessionHelper.GetAllAudioSessionsList(audioSessions);

            var res = from sc in _appSettings.AudioSessions
                      join session in sessions on sc.Id equals session.ProcessName
                      select new Device
                      {
                          Id = session.ProcessID,
                          Name = sc.Name,
                          Volume = session.Volume,
                          MaxPain = sc.MaxPain,
                          MaxVolume = sc.MaxVolume,
                          Process = sc.Id,
                          SoundCard = session.DeviceName
                      };


            return res.ToList();
        }
        public Device GetDevice(uint processId, string deviceName)
        {
            var audioSessions = _appSettings.AudioSessions.Where(s => s.DeviceName == deviceName).ToList();
            var session = _audioSessionHelper.GetAllAudioSessionsList(audioSessions).SingleOrDefault(s => s.ProcessID == processId);
            if (session == null) return null;
            var sc = audioSessions.Single(s => s.Id == session.ProcessName);
            return new Device
            {
                Id = session.ProcessID,
                Name = sc.Name,
                Volume = session.Volume,
                MaxPain = sc.MaxPain,
                MaxVolume = sc.MaxVolume,
                Process = sc.Id,
                SoundCard = session.DeviceName
            };
        }

        internal void ResetDevices()
        {
            var audioSessions = _appSettings.AudioSessions.ToList();
            _audioSessionHelper.SetDefaults(audioSessions);
        }

        internal void SetVolume(uint processId, string deviceName, float newVolume)
        {
            _audioSessionHelper.SetSessionVolume(processId, deviceName, newVolume);
        }
    }

    public class Device
    {
        public required uint Id { get; set; }
        public string SoundCard { get; set; }
        public string Process { get; set; }
        public required string Name { get; set; }
        public required float Volume { get; set; }
        public float MaxVolume { get; internal set; }
        public float MaxPain { get; internal set; }
    }
}