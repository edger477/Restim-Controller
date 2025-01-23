using Microsoft.AspNetCore.Mvc;
using RestimController.Models;

namespace RestimController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestimController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        public RestimController(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        [HttpGet("volume")]
        public IActionResult GetVolume()
        {
            // var device = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            // float volume = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
            return Ok(new { volume = 0.0 });
        }

        [HttpGet("instances")]
        public IActionResult GetInstances()
        {
            var instances = _appSettings.RestimInstances.Where(r => r.Enabled).ToList();

            return Ok(new { instances });
        }


        [HttpPost("volume")]
        public IActionResult SetVolume([FromBody] VolumeRequest request)
        {
            var restim = _appSettings.RestimInstances.First(r => r.Id == request.Id);
            restim.NewVolume = request.Volume;
            return Ok(new { });
        }

        [HttpPost("{restimInstance}/spike")]
        public IActionResult Spike(string restimInstance, [FromBody] SpikeRequest spike)
        {

            var instance = _appSettings.RestimInstances.FirstOrDefault(i => i.Id == restimInstance);
            if (instance != null)
            {
                instance.CurrentSpike = new RestimInstance.Spiking
                {
                    Intensity = spike.Amount,
                    OnTime = spike.MillisecondsOn,
                    OffTime = spike.MillisecondsOff,
                    ToRepeat = spike.RepeatCount
                };
            }
            return Ok(new { });
        }

        [HttpPost("{restimInstance}/pause/{duration:int}")]
        public IActionResult Pause(string restimInstance, int duration)
        {
            var instance = _appSettings.RestimInstances.FirstOrDefault(i => i.Id == restimInstance);
            if (instance != null)
            {
                var oldVolume = instance.Volume;
                instance.NewVolume = 0;
                if (duration > 0)
                {
                    instance.NewDelayedVolume = oldVolume;
                    instance.NewDelayedVolumeSetAt = DateTime.Now.AddSeconds(duration);
                }
            }
            return Ok(new { });
        }


        [HttpPost("{restimInstance}/axis")]
        public IActionResult SetAxis(string restimInstance, [FromBody] AxisData request)
        {
            var instance = _appSettings.RestimInstances.FirstOrDefault(i => i.Id == restimInstance);
            if (instance != null)
            {
                var axis = instance.Axes.FirstOrDefault(a => a.Name == request.Name);
                if (axis != null)
                {
                    axis.NewFrequency = request.NewFrequency;
                    axis.Offset = request.Offset;
                    axis.Value = request.NewValue;
                }
            }
            return Ok(new { });
        }
    }
}
