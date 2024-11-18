using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NAudio.CoreAudioApi;
using VolumeControl.Services;

namespace VolumeControl.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VolumeController : ControllerBase
    {
        private readonly AudioUtils _audioUtils;
        private readonly AppSettings _appSettings;

        public VolumeController(AppSettings appSettings, AudioUtils audioUtils)
        {
            _audioUtils = audioUtils;
            _appSettings = appSettings;
        }

        [HttpGet("volume")]
        public IActionResult GetVolume()
        {
            // var device = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            // float volume = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
            return Ok(new { volume = 0.0 });
        }

        [HttpGet("devices")]
        public IActionResult GetDevices()
        {
            var devices = _audioUtils.GetDevices();

            return Ok(new { devices });
        }

        [HttpPost("reset-devices")]
        public IActionResult SetDeviceDefaults()
        {
            _audioUtils.ResetDevices();
            return Ok(GetDevices());
        }

        [HttpPost("set")]
        public IActionResult SetVolume([FromBody] VolumeRequest request)
        {
            _audioUtils.SetVolume(request.Id, request.DeviceName, request.Volume);
            return Ok();
        }
        [HttpPost("spike")]
        public IActionResult Spike([FromBody] SpikeRequest request)
        {
            var device = _audioUtils.GetDevice(request.Id, request.DeviceName);

            Response.OnCompleted(async () =>
            {
                for (int i = 0; i < request.RepeatCount; i++)
                {
                    _audioUtils.SetVolume(request.Id, request.DeviceName, device.Volume + request.Amount);
                    await Task.Delay(TimeSpan.FromMilliseconds(request.MillisecondsOn));
                    _audioUtils.SetVolume(request.Id, request.DeviceName, device.Volume);
                    await Task.Delay(TimeSpan.FromMilliseconds(request.MillisecondsOff));
                }
            });
            return Ok();
        }
        [HttpPost("pause")]
        public IActionResult Pause([FromBody] PauseRequest request)
        {
            var device = _audioUtils.GetDevice(request.Id, request.DeviceName);

            Response.OnCompleted(async () =>
            {
                _audioUtils.SetVolume(request.Id, request.DeviceName, 0);
                if (request.DurationSeconds > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(request.DurationSeconds));
                    await ResumeSignal(new VolumeRequest() { DeviceName = request.DeviceName, Id = request.Id, Volume = device.Volume });
                }
                // _audioUtils.SetVolume(request.Id, request.DeviceName, device.Volume * .7F);
                // await Task.Delay(TimeSpan.FromSeconds(1));
                // _audioUtils.SetVolume(request.Id, request.DeviceName, device.Volume * .8F);
                // await Task.Delay(TimeSpan.FromSeconds(1));
                // _audioUtils.SetVolume(request.Id, request.DeviceName, device.Volume * .9F);
                // await Task.Delay(TimeSpan.FromSeconds(1));
                // _audioUtils.SetVolume(request.Id, request.DeviceName, device.Volume);
            });
            return Ok();
        }

        [HttpPost("resume")]
        public IActionResult Resume([FromBody] VolumeRequest request)
        {
            Response.OnCompleted(async () =>
            {
                await ResumeSignal(request);
            });
            return Ok();
        }

        private async Task ResumeSignal(VolumeRequest request)
        {
            _audioUtils.SetVolume(request.Id, request.DeviceName, request.Volume * .7F);
            await Task.Delay(TimeSpan.FromSeconds(1));
            _audioUtils.SetVolume(request.Id, request.DeviceName, request.Volume * .8F);
            await Task.Delay(TimeSpan.FromSeconds(1));
            _audioUtils.SetVolume(request.Id, request.DeviceName, request.Volume * .9F);
            await Task.Delay(TimeSpan.FromSeconds(1));
            _audioUtils.SetVolume(request.Id, request.DeviceName, request.Volume);

        }
    }

    public class VolumeRequest
    {
        public uint Id { get; set; }
        public string DeviceName { get; set; }
        public float Volume { get; set; }
    }
    public class SpikeRequest
    {
        public uint Id { get; set; }
        public string DeviceName { get; set; }
        public float Amount { get; set; }

        public int MillisecondsOn { get; set; }
        public int MillisecondsOff { get; set; }

        public int RepeatCount { get; set; }
    }
    public class PauseRequest
    {
        public uint Id { get; set; }
        public string DeviceName { get; set; }
        public int DurationSeconds { get; set; }
    }
}
