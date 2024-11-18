using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;

namespace VolumeControl.Services
{
    public class AudioSessionHelper
    {
        private readonly MMDeviceEnumerator _deviceEnumerator;

        public AudioSessionHelper()
        {
            // Initialize the device enumerator
            _deviceEnumerator = new MMDeviceEnumerator();
        }

        public List<AudioSession> GetAllAudioSessionsList(List<AudioSessionConfig> audioSessions)
        {
            // Enumerate all active playback devices
            var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            List<AudioSession> result = new List<AudioSession>();
            foreach (var device in devices)
            {
                var session = audioSessions.FirstOrDefault(s => s.DeviceName == device.FriendlyName);
                if (session == null) continue;

                // Get the session manager for the device
                var sessionManager = device.AudioSessionManager;
                for (int i = 0; i < sessionManager.Sessions.Count; i++)
                {
                    var control = sessionManager.Sessions[i];

                    if (control.GetSessionIdentifier.Contains(session.Id))
                    {
                        result.Add(new AudioSession
                        {
                            DeviceName = device.FriendlyName,
                            DisplayName = control.DisplayName,
                            ProcessID = control.GetProcessID,
                            ProcessName = session.Id,
                            SessionIdentifier = control.GetSessionIdentifier,
                            Volume = control.SimpleAudioVolume.Volume, // Convert volume to percentage
                            IsMuted = control.SimpleAudioVolume.Mute
                        });
                    }
                }
            }
            return result;
        }
        public IEnumerable<AudioSession> GetAllAudioSessions(List<AudioSessionConfig> audioSessions)
        {
            var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            List<AudioSession> result = new List<AudioSession>();
            foreach (var device in devices)
            {
                var session = audioSessions.FirstOrDefault(s => s.DeviceName == device.FriendlyName);
                if (session == null) continue;

                // Get the session manager for the device
                var sessionManager = device.AudioSessionManager;
                for (int i = 0; i < sessionManager.Sessions.Count; i++)
                {
                    var control = sessionManager.Sessions[i];

                    if (control.GetSessionIdentifier.Contains(session.Id))
                    {
                        yield return new AudioSession
                        {
                            DeviceName = device.FriendlyName,
                            DisplayName = control.DisplayName,
                            ProcessID = control.GetProcessID,
                            ProcessName = session.Id,
                            SessionIdentifier = control.GetSessionIdentifier,
                            Volume = control.SimpleAudioVolume.Volume, // Convert volume to percentage
                            IsMuted = control.SimpleAudioVolume.Mute
                        };
                    }
                }
            }
        }

        public void SetSessionVolume(uint processId, string deviceName, float volume)
        {
            var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            foreach (var device in devices.Where(d => d.FriendlyName == deviceName))
            {
                var sessionManager = device.AudioSessionManager;

                for (int i = 0; i < sessionManager.Sessions.Count; i++)
                {
                    {
                        var control = sessionManager.Sessions[i];
                        if (control.GetProcessID == processId)
                        {
                            control.SimpleAudioVolume.Volume = volume; // Set volume (0.0 to 1.0)
                            return; // Exit once the session is found and adjusted
                        }
                    }
                }
            }
        }

        public void MuteSession(int processId, bool mute)
        {
            var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            foreach (var device in devices)
            {
                var sessionManager = device.AudioSessionManager;

                for (int i = 0; i < sessionManager.Sessions.Count; i++)
                {
                    {
                        var control = sessionManager.Sessions[i];
                        if (control.GetProcessID == processId)
                        {
                            control.SimpleAudioVolume.Mute = mute;
                            return; // Exit once the session is found and adjusted
                        }
                    }
                }
            }
        }

        internal void SetDefaults(List<AudioSessionConfig> audioSessions)
        {
            var sessions = GetAllAudioSessions(audioSessions);
            foreach (var session in sessions)
            {
                var config = audioSessions.Single(s => s.Id == session.ProcessName);
                if (session.Volume >= config.MaxVolume)
                {
                    SetSessionVolume(session.ProcessID, config.DeviceName, config.MaxVolume);
                }

            }
        }

        public class AudioSession
        {
            public string DeviceName { get; set; }
            public string DisplayName { get; set; }
            public uint ProcessID { get; set; }
            public string ProcessName { get; internal set; }
            public string SessionIdentifier { get; set; }
            public float Volume { get; set; }
            public bool IsMuted { get; set; }
        }
    }
}
