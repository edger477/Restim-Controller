using RestimController.Models;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace RestimController.Services
{
    public class CommandSender : BackgroundService
    {
        private readonly int _updatesPerSecond = 60;
        private readonly AppSettings _appSettings;

        public CommandSender(AppSettings appSettings)
        {
            _appSettings = appSettings;
            foreach (var restim in _appSettings.RestimInstances)
            {
                restim.NewVolume = restim.Volume;
                foreach (var axis in restim.Axes)
                {
                    axis.NewFrequency = axis.Frequency;
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _appSettings.RestimInstances.Where(r => r.Enabled).AsParallel().ForAll(async (restimInstance) =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var client = new TcpClient();
                    try
                    {
                        var masterAxis = new AxisData()
                        {
                            TCodeAxis = restimInstance.MasterVolumeTCodeAxis,
                            ValueMax = 0.999,
                            ValueMin = 0
                        };
                        // Connect to the specified server
                        await client.ConnectAsync(restimInstance.Host, restimInstance.Port);


                        using var stream = client.GetStream();
                        restimInstance.IsConnected = true;
                        restimInstance.ErrorMessage = null;
                        while (!stoppingToken.IsCancellationRequested)
                        {
                            var elapsedTime = stopwatch.ElapsedMilliseconds / 1000d;
                            if (restimInstance.NewDelayedVolume.HasValue && restimInstance.NewDelayedVolumeSetAt.HasValue && restimInstance.NewDelayedVolumeSetAt.Value < DateTime.Now)
                            {
                                restimInstance.NewVolume = restimInstance.NewDelayedVolume.Value;

                                restimInstance.NewDelayedVolume = null;
                                restimInstance.NewDelayedVolumeSetAt = null;
                            }

                            if (restimInstance.CurrentSpike != null)
                            {
                                var currentSpike = restimInstance.CurrentSpike;
                                if (!currentSpike.NextOff.HasValue && !currentSpike.NextOn.HasValue && currentSpike.IsOn == false)
                                {
                                    currentSpike.NextOn = DateTime.Now;
                                    currentSpike.NextOff = DateTime.Now.AddMilliseconds(currentSpike.OnTime);
                                }

                                if (currentSpike.NextOn <= DateTime.Now && currentSpike.NextOff > currentSpike.NextOn)
                                {
                                    currentSpike.NextOn = currentSpike.NextOn.Value.AddMilliseconds(currentSpike.OnTime + currentSpike.OffTime);
                                    currentSpike.ToRepeat -= 1;
                                    currentSpike.IsOn = true;
                                    var packet = CreateAxisValuePacket(masterAxis, restimInstance.Volume + currentSpike.Intensity);
                                    await stream.WriteAsync(packet, stoppingToken);

                                }
                                else if (currentSpike.NextOff <= DateTime.Now && currentSpike.NextOn > currentSpike.NextOff)
                                {
                                    currentSpike.NextOff = currentSpike.NextOff.Value.AddMilliseconds(currentSpike.OnTime + currentSpike.OffTime);
                                    currentSpike.IsOn = false;
                                    if (currentSpike.ToRepeat <= 0)
                                    {
                                        restimInstance.CurrentSpike = null;
                                    }
                                    var packet = CreateAxisValuePacket(masterAxis, restimInstance.Volume);
                                    await stream.WriteAsync(packet, stoppingToken);
                                }

                            }
                            else if (restimInstance.Volume > restimInstance.NewVolume)
                            {
                                var packet = CreateAxisValuePacket(masterAxis, restimInstance.NewVolume);
                                await stream.WriteAsync(packet, stoppingToken);
                                restimInstance.Volume = restimInstance.NewVolume;
                            }
                            else if (restimInstance.Volume < restimInstance.NewVolume)
                            {
                                restimInstance.Volume = Math.Min(restimInstance.Volume + 0.1f / _updatesPerSecond, restimInstance.NewVolume); // update volume up max 5% per second
                                var packet = CreateAxisValuePacket(masterAxis, restimInstance.Volume);
                                await stream.WriteAsync(packet, stoppingToken);
                            }
                            foreach (var axis in restimInstance.Axes)
                            {
                                double value = axis.Value;
                                if (axis.Frequency > 0)
                                {
                                    // Calculate oscillating value
                                    double range = axis.ValueMax - axis.ValueMin;
                                    double midpoint = axis.ValueMin + range / 2;
                                    double amplitude = range / 2;

                                    value = midpoint + amplitude * Math.Sin(2 * Math.PI * axis.Frequency * (elapsedTime - axis.Offset));
                                    if (axis.Frequency != axis.NewFrequency)
                                    {
                                        var newValue = midpoint + amplitude * Math.Sin(2 * Math.PI * axis.NewFrequency * (elapsedTime - axis.Offset));
                                        if (Math.Abs(newValue - value) < 0.1)
                                        {
                                            value = (value + newValue) / 2;
                                            axis.Frequency = axis.NewFrequency;
                                        }
                                    }
                                    axis.Value = value;
                                }
                                // Create the TCode packet
                                byte[] data = CreateAxisValuePacket(axis, value);

                                // Send the packet to the server
                                await stream.WriteAsync(data, stoppingToken);
                            }

                            // Sleep to maintain 60 commands per second
                            await Task.Delay(1000 / _updatesPerSecond, stoppingToken);
                        }
                        await Task.Delay(1000, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        restimInstance.ErrorMessage = ex.Message;
                        restimInstance.IsConnected = false;
                    }
                }
            });
        }

        private static byte[] CreateAxisValuePacket(AxisData axis, double value)
        {
            var range = axis.ValueMax - axis.ValueMin;
            var scaledValue = Math.Min(value / range, 0.999);
            var intvalue = (int)Math.Round(scaledValue * 1000);
            string packet = $"{axis.TCodeAxis}{intvalue:D3}";
            byte[] data = Encoding.ASCII.GetBytes(packet + Environment.NewLine);
            return data;
        }
    }
}